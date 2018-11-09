// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CSharpE.Syntax.Internals;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using RoslynSyntaxTree = Microsoft.CodeAnalysis.SyntaxTree;
using InternalSyntax = Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

namespace CSharpE.Transform.VisualStudio
{
    internal class SyntaxTree : CSharpSyntaxTree
    {
        private readonly CSharpParseOptions _options;
        private readonly string _path;
        private readonly CSharpSyntaxNode _root;
        private readonly bool _hasCompilationUnitRoot;
        private readonly Encoding _encodingOpt;
        private readonly SourceHashAlgorithm _checksumAlgorithm;
        private SourceText _lazyText;
        private InternalSyntax.DirectiveStack _directives;

        // TODO: can text be lazy?
        // TODO: directives?
        public SyntaxTree(CSharpSyntaxTree roslynTree)
            : this(roslynTree.GetText(), roslynTree.Encoding, roslynTree.GetText().ChecksumAlgorithm, roslynTree.FilePath, roslynTree.Options, roslynTree.GetRoot(), default) { }

        internal SyntaxTree(SourceText textOpt, Encoding encodingOpt, SourceHashAlgorithm checksumAlgorithm, string path, CSharpParseOptions options, CSharpSyntaxNode root, InternalSyntax.DirectiveStack directives)
        {
            Debug.Assert(root != null);
            Debug.Assert(options != null);
            Debug.Assert(textOpt == null || textOpt.Encoding == encodingOpt && textOpt.ChecksumAlgorithm == checksumAlgorithm);

            _lazyText = textOpt;
            _encodingOpt = encodingOpt ?? textOpt?.Encoding;
            _checksumAlgorithm = checksumAlgorithm;
            _options = options;
            _path = path ?? string.Empty;
            _root = this.CloneNodeAsRoot(Annotate(SyntaxNode.CloneNodeAsRoot(root, null)));
            _hasCompilationUnitRoot = root.Kind() == SyntaxKind.CompilationUnit;
            _directives = directives;
            this.SetDirectiveStack(directives);
        }

        internal static TNode Annotate<TNode>(TNode node) where TNode : SyntaxNode
            => node.ReplaceNodes(
                node.DescendantNodes().Where(n => Annotation.Get(n) == null),
                (_, n) => n.WithAdditionalAnnotations(Annotation.Create()));

        public override string FilePath
        {
            get { return _path; }
        }

        public override SourceText GetText(CancellationToken cancellationToken)
        {
            if (_lazyText == null)
            {
                Interlocked.CompareExchange(ref _lazyText, this.GetRoot(cancellationToken).GetText(_encodingOpt, _checksumAlgorithm), null);
            }

            return _lazyText;
        }

        public override bool TryGetText(out SourceText text)
        {
            text = _lazyText;
            return text != null;
        }

        public override Encoding Encoding
        {
            get { return _encodingOpt; }
        }

        public override int Length
        {
            get { return _root.FullSpan.Length; }
        }

        public override CSharpSyntaxNode GetRoot(CancellationToken cancellationToken)
        {
            return _root;
        }

        public override bool TryGetRoot(out CSharpSyntaxNode root)
        {
            root = _root;
            return true;
        }

        public override bool HasCompilationUnitRoot
        {
            get
            {
                return _hasCompilationUnitRoot;
            }
        }

        public override CSharpParseOptions Options
        {
            get
            {
                return _options;
            }
        }

        public override SyntaxReference GetReference(SyntaxNode node)
        {
            return new SimpleSyntaxReference(node);
        }

        public override RoslynSyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
        {
            if (ReferenceEquals(_root, root) && ReferenceEquals(_options, options))
            {
                return this;
            }

            return new SyntaxTree(
                null,
                _encodingOpt,
                _checksumAlgorithm,
                _path,
                (CSharpParseOptions)options,
                (CSharpSyntaxNode)root,
                _directives);
        }

        public override RoslynSyntaxTree WithFilePath(string path)
        {
            if (_path == path)
            {
                return this;
            }

            return new SyntaxTree(
                _lazyText,
                _encodingOpt,
                _checksumAlgorithm,
                path,
                _options,
                _root,
                _directives);
        }

        public override RoslynSyntaxTree WithChangedText(SourceText newText) => new SyntaxTree((CSharpSyntaxTree)base.WithChangedText(newText));
    }
}
