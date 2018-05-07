using System;

namespace CSharpE.Syntax.Internals.Mapping
{
    internal interface ISyntaxMapping<TSyntax, TRoslynSyntax>
        where TSyntax : ISyntax<TRoslynSyntax>
        where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode
    {
        TRoslynSyntax GetWrapped(TSyntax syntax);
    }

    internal class SyntaxMapping<TSyntax, TRoslynSyntax, TRoslynMembers> : ISyntaxMapping<TSyntax, TRoslynSyntax>
        where TSyntax : ISyntax<TRoslynSyntax>
        where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode 
    {
        private readonly SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembers> members;
        private readonly Func<TRoslynMembers, TRoslynSyntax> constructor;

        public SyntaxMapping(
            SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembers> members,
            Func<TRoslynMembers, TRoslynSyntax> constructor)
        {
            this.members = members;
            this.constructor = constructor;
        }

        public TRoslynSyntax GetWrapped(TSyntax syntax) => members.GetWrapped(syntax, constructor);

        public void Push(TSyntax syntax, TRoslynSyntax roslynSyntax) => members.Push(syntax, roslynSyntax);
    }

    internal class SyntaxMapping
    {
        public static SyntaxMappingBuilder<TSyntax, TRoslynSyntax, Unit> For<TSyntax, TRoslynSyntax>(
            RefFunc<TSyntax, TRoslynSyntax> syntaxAccessor)
            where TSyntax : ISyntax<TRoslynSyntax>
            where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode =>
            new SyntaxMappingBuilder<TSyntax, TRoslynSyntax, Unit>(
                new SyntaxMappingMembersLeaf<TSyntax, TRoslynSyntax>(syntaxAccessor));
    }

    internal class SyntaxMappingBuilder<TSyntax, TRoslynSyntax, TRoslynMembers>
        where TSyntax : ISyntax<TRoslynSyntax>
        where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode
    {
        private readonly SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembers> members;

        public SyntaxMappingBuilder(SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembers> members) =>
            this.members = members;

        public SyntaxMappingBuilder<TSyntax, TRoslynSyntax, (TMemberRoslynSyntax, TRoslynMembers)>
            AddMember<TMemberSyntax, TMemberRoslynSyntax>(
                Func<TSyntax, TMemberSyntax> memberAccessor, Func<TSyntax, TMemberRoslynSyntax> roslynMemberAccessor,
                Func<TRoslynSyntax, TMemberRoslynSyntax> roslynMemberDeconstructor)
            where TMemberSyntax : ISyntax<TMemberRoslynSyntax>
            where TMemberRoslynSyntax : class
        {
            return new SyntaxMappingBuilder<TSyntax, TRoslynSyntax, (TMemberRoslynSyntax, TRoslynMembers)>(
                new SyntaxMappingMembersNode<TSyntax, TRoslynSyntax, TMemberSyntax, TMemberRoslynSyntax, TRoslynMembers>(
                    memberAccessor, roslynMemberAccessor, roslynMemberDeconstructor, members));
        }

        internal ISyntaxMapping<TSyntax, TRoslynSyntax> BuildCore(Func<TRoslynMembers, TRoslynSyntax> constructor) =>
            new SyntaxMapping<TSyntax, TRoslynSyntax, TRoslynMembers>(members, constructor);
    }

    internal abstract class SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembers>
        where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode
    {
        public TRoslynSyntax GetWrapped(TSyntax syntax, Func<TRoslynMembers, TRoslynSyntax> constructor)
        {
            var (members, changed) = GetRoslynMembers(syntax);

            if (!changed)
                return null;

            return constructor(members);
        }

        internal abstract (TRoslynMembers members, bool changed) GetRoslynMembers(TSyntax syntax);

        public abstract void Push(TSyntax syntax, TRoslynSyntax roslynSyntax);
    }

    internal class SyntaxMappingMembersNode<TSyntax, TRoslynSyntax, TMember, TRoslynMember, TRoslynMembersRest>
        : SyntaxMappingMembers<TSyntax, TRoslynSyntax, (TRoslynMember head, TRoslynMembersRest tail)>
        where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode
        where TMember : ISyntax<TRoslynMember>
        where TRoslynMember : class
    {
        private readonly Func<TSyntax, TMember> memberAccessor;
        private readonly Func<TSyntax, TRoslynMember> roslynMemberAccessor;
        private readonly Func<TRoslynSyntax, TRoslynMember> roslynMemberDeconstructor;
        private readonly SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembersRest> previousMembers;

        public SyntaxMappingMembersNode(
            Func<TSyntax, TMember> memberAccessor, Func<TSyntax, TRoslynMember> roslynMemberAccessor,
            Func<TRoslynSyntax, TRoslynMember> roslynMemberDeconstructor,
            SyntaxMappingMembers<TSyntax, TRoslynSyntax, TRoslynMembersRest> previousMembers)
        {
            this.memberAccessor = memberAccessor;
            this.roslynMemberAccessor = roslynMemberAccessor;
            this.roslynMemberDeconstructor = roslynMemberDeconstructor;
            this.previousMembers = previousMembers;
        }

        internal override ((TRoslynMember head, TRoslynMembersRest tail) members, bool changed) GetRoslynMembers(
            TSyntax syntax)
        {
            var member = memberAccessor(syntax);

            var oldRoslynMember = roslynMemberAccessor(syntax);

            var newRoslynMember = member?.GetWrapped() ?? oldRoslynMember;

            var (previousRoslynMembers, previousChanged) = previousMembers.GetRoslynMembers(syntax);

            return ((newRoslynMember, previousRoslynMembers), previousChanged || newRoslynMember != oldRoslynMember);
        }

        public override void Push(TSyntax syntax, TRoslynSyntax roslynSyntax)
        {
            var roslynMember = roslynMemberDeconstructor(roslynSyntax);
            var member = memberAccessor(syntax);

            member.Push(roslynMember);

            previousMembers.Push(syntax, roslynSyntax);
        }
    }

    internal delegate ref TResult RefFunc<T, TResult>(T arg);

    internal class SyntaxMappingMembersLeaf<TSyntax, TRoslynSyntax>
        : SyntaxMappingMembers<TSyntax, TRoslynSyntax, Unit>
        where TRoslynSyntax : Microsoft.CodeAnalysis.SyntaxNode
    {
        private readonly RefFunc<TSyntax, TRoslynSyntax> syntaxAccessor;

        public SyntaxMappingMembersLeaf(RefFunc<TSyntax, TRoslynSyntax> syntaxAccessor) =>
            this.syntaxAccessor = syntaxAccessor;

        internal override (Unit members, bool changed) GetRoslynMembers(TSyntax syntax) => (default, false);

        public override void Push(TSyntax syntax, TRoslynSyntax roslynSyntax) => syntaxAccessor(syntax) = roslynSyntax;
    }
}