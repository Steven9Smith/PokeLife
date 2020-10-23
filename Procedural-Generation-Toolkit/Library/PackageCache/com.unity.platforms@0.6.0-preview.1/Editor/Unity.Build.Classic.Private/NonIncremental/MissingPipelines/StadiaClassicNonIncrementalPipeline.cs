/* todo: bring back stadia

using Bee.Toolchain.GGP;
using Unity.BuildSystem.NativeProgramSupport;
using UnityEditor;

namespace Unity.Build.Classic.Private.MissingPipelines
{
    /// <summary>
    /// Placeholder classic non incremental pipeline for Stadia
    /// Note: Should be remove when a proper implementation is done.
    /// </summary>
    class StadiaClassicNonIncrementalPipeline : MissingNonIncrementalPipeline
    {
        public override Platform Platform => new GGPPlatform();

        protected override BuildTarget BuildTarget => BuildTarget.Stadia;
    }
}

*/
