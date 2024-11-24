// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

public class TaikoPatternFields
{
    /// <summary>
    /// Stores rhythm alignment amplitudes between all notes
    /// </summary>
    public TaikoTimeField RhythmField = new TaikoTimeField();

    /// <summary>
    /// Stores rhythm alignment amplitudes between centre (don) notes
    /// </summary>
    public TaikoTimeField CentreField = new TaikoTimeField();

    /// <summary>
    /// Stores rhythm alignment amplitudes between rim (katsu) notes
    /// </summary>
    public TaikoTimeField RimField = new TaikoTimeField();

    /// <summary>
    /// Stores rhythm alignment amplitudes between colour changes
    /// </summary>
    public TaikoTimeField ColourChangeField = new TaikoTimeField();
}
