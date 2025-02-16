
// SPDX-License-Identifier: GPL-3.0-or-later


using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SnapX.Core.Utils.Extensions;

namespace SnapX.Core.ImageEffects.Adjustments;
internal class Gamma : ImageEffect
{
    [DefaultValue(1f), Description("Min 0.1, Max 5.0")]
    public float Value { get; set; }

    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public Gamma()
    {
        this.ApplyDefaultPropertyValues();
    }

    public override Image Apply(Image img)
    {
        img.Mutate(ctx => ctx.ApplyGamma(Value));

        return img;
    }

    protected override string GetSummary()
    {
        return Value.ToString();
    }
}
