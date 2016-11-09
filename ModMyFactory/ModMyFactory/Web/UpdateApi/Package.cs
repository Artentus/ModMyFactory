using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModMyFactory.Web.UpdateApi
{
    sealed class Package : IReadOnlyList<UpdateStep>
    {
        readonly List<UpdateStep> updateSteps;

        public int Count => updateSteps.Count;

        public UpdateStep this[int index] => updateSteps[index];

        public Package(UpdateStepTemplate[] templates)
        {
            updateSteps = new List<UpdateStep>(templates.Length);

            Version stableVersion = templates.FirstOrDefault(template => template.Stable != null)?.Stable;
            foreach (var template in templates)
            {
                if (template.Stable == null)
                {
                    bool isStable = (stableVersion != null) && (template.To == stableVersion);
                    updateSteps.Add(new UpdateStep(template.From, template.To, isStable));
                }
            }
        }

        public IEnumerator<UpdateStep> GetEnumerator()
        {
            return updateSteps.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
