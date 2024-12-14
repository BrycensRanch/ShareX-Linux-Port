﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2024 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using System.IO.Pipes;
using System.Text;

namespace ShareX.Core.Utils.Miscellaneous;

public class SingleInstanceManager : IDisposable
{
    public event Action<string[]> ArgumentsReceived;

    public string MutexName { get; private set; }
    public string PipeName { get; private set; }
    public bool IsSingleInstance { get; private set; }
    public bool IsFirstInstance { get; private set; }

    private const int MaxArgumentsLength = 100;
    private const int ConnectTimeout = 5000;

    private readonly Mutex mutex;
    private CancellationTokenSource cts;

    public SingleInstanceManager(string mutexName, string pipeName, string[] args) : this(mutexName, pipeName, true, args)
    {
    }

    public SingleInstanceManager(string mutexName, string pipeName, bool isSingleInstance, string[] args)
    {
        MutexName = mutexName;
        PipeName = pipeName;
        IsSingleInstance = isSingleInstance;

        mutex = new Mutex(false, MutexName);

        try
        {
            IsFirstInstance = mutex.WaitOne(100, false);

            if (IsSingleInstance)
            {
                if (IsFirstInstance)
                {
                    cts = new CancellationTokenSource();

                    System.Threading.Tasks.Task.Run(ListenForConnectionsAsync, cts.Token);
                }
                else
                {
                    RedirectArgumentsToFirstInstance(args);
                }
            }
        }
        catch (AbandonedMutexException)
        {
            DebugHelper.WriteLine("Single instance mutex found abandoned from another process.");

            IsFirstInstance = true;
        }
    }

    protected virtual void OnArgumentsReceived(string[] arguments)
    {
        if (ArgumentsReceived != null)
        {
            System.Threading.Tasks.Task.Run(() => ArgumentsReceived?.Invoke(arguments));
        }
    }

    private async System.Threading.Tasks.Task ListenForConnectionsAsync()
    {
        while (!cts.IsCancellationRequested)
        {
            var namedPipeServerCreated = false;

            try
            {
                await using var namedPipeServer = new NamedPipeServerStream(PipeName, PipeDirection.InOut,
                    1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                namedPipeServerCreated = true;

                await namedPipeServer.WaitForConnectionAsync(cts.Token).ConfigureAwait(false);

                using var reader = new BinaryReader(namedPipeServer, Encoding.UTF8);

                var length = reader.ReadInt32();

                if (length < 0 || length > MaxArgumentsLength)
                {
                    throw new Exception("Invalid length: " + length);
                }

                var args = new string[length];

                for (int i = 0; i < length; i++)
                {
                    args[i] = reader.ReadString();
                }

                OnArgumentsReceived(args);


            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                DebugHelper.WriteException(e);

                if (!namedPipeServerCreated)
                {
                    break;
                }
            }
        }
    }

    private void RedirectArgumentsToFirstInstance(string[] args)
    {
        try
        {
            using var namedPipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

            namedPipeClient.Connect(ConnectTimeout);

            using var writer = new BinaryWriter(namedPipeClient, Encoding.UTF8);
            writer.Write(args.Length);

            foreach (string argument in args)
            {
                writer.Write(argument);
            }
        }
        catch (Exception e)
        {
            DebugHelper.WriteException(e);
        }
    }

    public void Dispose()
    {
        if (cts != null)
        {
            cts.Cancel();
            cts.Dispose();
        }

        if (mutex != null)
        {
            if (IsFirstInstance)
            {
                mutex.ReleaseMutex();
            }

            mutex.Dispose();
        }
    }
}

