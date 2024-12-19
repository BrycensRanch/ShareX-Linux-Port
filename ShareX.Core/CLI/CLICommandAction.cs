
// SPDX-License-Identifier: GPL-3.0-or-later


namespace ShareX.Core.CLI;
public class CLICommandAction
{
    public string[] Commands;
    public Action DefaultAction;
    public Action<string> TextAction;
    public Action<int> NumberAction;

    public CLICommandAction(params string[] commands)
    {
        Commands = commands;
    }

    public bool CheckCommands(List<CLICommand> commands)
    {
        foreach (CLICommand command in commands)
        {
            foreach (string text in Commands)
            {
                if (command.CheckCommand(text))
                {
                    ExecuteAction(command.Parameter);
                    return true;
                }
            }
        }

        return false;
    }

    private void ExecuteAction(string parameter)
    {
        if (DefaultAction != null)
        {
            DefaultAction();
        }
        else if (!string.IsNullOrEmpty(parameter))
        {
            if (TextAction != null)
            {
                TextAction(parameter);
            }
            else if (NumberAction != null)
            {
                if (int.TryParse(parameter, out int num))
                {
                    NumberAction(num);
                }
            }
        }
    }
}

