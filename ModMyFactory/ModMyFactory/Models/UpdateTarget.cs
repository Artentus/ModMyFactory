using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ModMyFactory.Web.UpdateApi;

namespace ModMyFactory.Models
{
    sealed class UpdateTarget
    {
        public IReadOnlyCollection<UpdateStep> Steps { get; }

        public Version TargetVersion { get; }

        public bool IsLatestStable { get; }

        public UpdateTarget(IList<UpdateStep> steps, Version targetVersion, bool isLatestStable)
        {
            Steps = new ReadOnlyCollection<UpdateStep>(steps);
            TargetVersion = targetVersion;
            IsLatestStable = isLatestStable;
        }
    }
}
