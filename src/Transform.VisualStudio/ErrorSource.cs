using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableManager;
using static Microsoft.VisualStudio.Shell.TableControl.StandardTableColumnDefinitions;

namespace CSharpE.Transform.VisualStudio
{
    internal class ErrorSource : ITableDataSource
    {
        internal static ErrorSource Instance { get; private set; }

        private readonly List<ITableDataSink> sinks = new List<ITableDataSink>();

        internal static void CreateInstance(ITableManagerProvider tableManagerProvider) =>
            Instance = new ErrorSource(tableManagerProvider);

        private ErrorSource(ITableManagerProvider tableManagerProvider)
        {
            var errorTableManager = tableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            errorTableManager.AddSource(this, ProjectName, ErrorSeverity, ErrorCode, Text);
        }

        string ITableDataSource.SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        string ITableDataSource.Identifier => nameof(CSharpE);

        string ITableDataSource.DisplayName => nameof(CSharpE);

        IDisposable ITableDataSource.Subscribe(ITableDataSink sink)
        {
            lock (sinks)
            {
                sinks.Add(sink);
            }

            return new Disposable(
                () =>
                {
                    lock (sinks)
                    {
                        sinks.Remove(sink);
                    }
                });
        }

        public void AddError(string code, string message, string project)
        {
            // TODO: race condition: what if sink is added after error?
            lock (sinks)
            {
                foreach (var sink in sinks)
                {
                    sink.AddEntries(new[] { new ErrorEntry(code, message, project) });
                }
            }
        }

        internal void ClearErrors()
        {
            lock (sinks)
            {
                foreach (var sink in sinks)
                {
                    sink.RemoveAllEntries();
                }
            }
        }

        private class Disposable : IDisposable
        {
            private readonly Action disposeAction;

            public Disposable(Action disposeAction) => this.disposeAction = disposeAction;

            public void Dispose() => disposeAction();
        }

        private class ErrorEntry : TableEntryBase
        {
            private readonly string code;
            private readonly string message;
            private readonly string project;

            public ErrorEntry(string code, string message, string project)
            {
                this.code = code;
                this.message = message;
                this.project = project;
            }

            public override bool TryGetValue(string keyName, out object content)
            {
                switch (keyName)
                {
                    case Text:
                        content = message;
                        return true;
                    case ProjectName:
                        content = project;
                        return true;
                    case ErrorSeverity:
                        content = __VSERRORCATEGORY.EC_ERROR;
                        return true;
                    case StandardTableKeyNames.TaskCategory:
                        content = VSTASKCATEGORY.CAT_BUILDCOMPILE;
                        return true;
                    case StandardTableKeyNames.ErrorCode:
                        content = code;
                        return true;
                    case StandardTableKeyNames.BuildTool:
                        content = nameof(CSharpE);
                        return true;
                }

                content = null;
                return false;
            }
        }
    }
}
