namespace CSharpE.Syntax.Internals
{
    internal struct ChangeTracker
    {
        private bool hasChanged;

        public void GetAndResetChanged(ref bool? changed)
        {
            if (changed == null)
                return;

            changed |= hasChanged;
            hasChanged = false;
        }

        public void SetChanged() => hasChanged = true;

        public void SetChanged(ref bool? changed)
        {
            if (changed == null)
                hasChanged = true;
            else
                changed = true;
        }
    }
}