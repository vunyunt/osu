// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public interface IApplicableToTaikoClassic : IApplicableMod
    {
        public void ApplyToTaikoModClassic(TaikoModClassic taikoModClassic);
    }
}