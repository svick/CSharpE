﻿using CSharpE.Syntax;

namespace CSharpE.Transform
{
    public interface ITransformation
    {
        void Process(Project project, bool designTime);
    }
}