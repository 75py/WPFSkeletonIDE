using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using WPFSkeletonIDE.Models;
using System.IO;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using System.Xml;
using Xceed.Wpf.AvalonDock.Layout;
using WPFSkeletonIDE.ViewModels.Panes;
using System.Reflection;

namespace WPFSkeletonIDE.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        #region DockingDocumentViewModels 変更通知プロパティ
        private ObservableCollection<ViewModel> _DockingDocumentViewModels;
        /// <summary>
        /// ドッキングドキュメントの ViewModel のリスト
        /// </summary>
        public ObservableCollection<ViewModel> DockingDocumentViewModels
        {
            get { return _DockingDocumentViewModels; }
            set
            {
                if (_DockingDocumentViewModels == value)
                    return;
                _DockingDocumentViewModels = value;
                RaisePropertyChanged(() => DockingDocumentViewModels);
            }
        }
        #endregion

        #region DockingPaneViewModels 変更通知プロパティ
        private ObservableCollection<ViewModel> _DockingPaneViewModels;
        /// <summary>
        /// ドッキングペインの ViewModel のリスト
        /// </summary>
        public ObservableCollection<ViewModel> DockingPaneViewModels
        {
            get { return _DockingPaneViewModels; }
            set
            {
                if (_DockingPaneViewModels == value)
                    return;
                _DockingPaneViewModels = value;
                RaisePropertyChanged(() => DockingPaneViewModels);
            }
        }
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            DockingDocumentViewModels = new ObservableCollection<ViewModel>();
            DockingDocumentViewModels.Add(new ViewModels.Documents.SourceFileDocumentViewModel());
            DockingDocumentViewModels.Add(new ViewModels.Documents.ProjectSettingDocumentViewModel());

            DockingPaneViewModels = new ObservableCollection<ViewModel>();
            DockingPaneViewModels.Add(new ViewModels.Panes.ErrorListPaneViewModel());
            DockingPaneViewModels.Add(new ViewModels.Panes.OutputPaneViewModel());
            DockingPaneViewModels.Add(new ViewModels.Panes.PropertyPaneViewModel());
            DockingPaneViewModels.Add(new ViewModels.Panes.SolutionExplorerPaneViewModel());
        }

        /// <summary>
        /// ウィンドウの初期化処理
        /// </summary>
        public void Initialize()
        {
        }

        String LayoutFile
        {
            get
            {
                return System.IO.Path.ChangeExtension(
                    Environment.GetCommandLineArgs()[0]
                    , ".AvalonDock.config"
                    );
            }
        }

        public void LoadLayout(DockingManager dockManager)
        {
            // backup default layout
            using (var ms = new MemoryStream())
            {
                var serializer = new XmlLayoutSerializer(dockManager);
                serializer.Serialize(ms);
            }

            // load file
            Byte[] bytes;
            try
            {
                bytes = System.IO.File.ReadAllBytes(LayoutFile);
            }
            catch (FileNotFoundException ex)
            {
                return;
            }

            // restore layout
            LoadLayoutFromBytes(dockManager, bytes);
        }

        bool LoadLayoutFromBytes(DockingManager dockManager, Byte[] bytes)
        {
            var serializer = new XmlLayoutSerializer(dockManager);

            serializer.LayoutSerializationCallback += MatchLayoutContent;

            try
            {
                using (var stream = new MemoryStream(bytes))
                {
                    serializer.Deserialize(stream);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        void MatchLayoutContent(object o, LayoutSerializationCallbackEventArgs e)
        {
            var contentId = e.Model.ContentId;

            if (e.Model is LayoutAnchorable)
            {
                foreach (var viewModel in DockingPaneViewModels)
                {
                    var propInfo = viewModel.GetType().GetProperty("ContentId", BindingFlags.Public | BindingFlags.Instance);
                    var vmContentId = (string)propInfo.GetValue(viewModel);
                    if (contentId == vmContentId)
                    {
                        e.Model.Content = viewModel;
                    }
                }
            }

            if (e.Model is LayoutDocument)
            {
                foreach (var viewModel in DockingDocumentViewModels)
                {
                    var propInfo = viewModel.GetType().GetProperty("ContentId", BindingFlags.Public | BindingFlags.Instance);
                    var vmContentId = (string)propInfo.GetValue(viewModel);
                    if (contentId == vmContentId)
                    {
                        e.Model.Content = viewModel;
                    }
                }
            }
        }

        public void SaveLayout(DockingManager dockManager)
        {
            var serializer = new XmlLayoutSerializer(dockManager);
            var doc = new XmlDocument();
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream);
                stream.Position = 0;
                doc.Load(stream);
            }
            using (var stream = new FileStream(LayoutFile, FileMode.Create))
            {
                doc.Save(stream);
            }
        }
    }
}
