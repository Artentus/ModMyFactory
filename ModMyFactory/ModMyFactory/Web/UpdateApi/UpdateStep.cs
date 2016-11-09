using System;

namespace ModMyFactory.Web.UpdateApi
{
    sealed class UpdateStep
    {
        public Version From { get; }

        public Version To { get; }

        public bool IsStable { get; }

        public UpdateStep(Version from, Version to, bool isStable)
        {
            From = from;
            To = to;
            IsStable = isStable;
        }
    }
}
