using System;
using System.Collections.Generic;
using ExShrinkSidebar.Script.Model;
using System.Windows;
using System.Windows.Controls;
using Point = System.Windows.Point;

namespace ExShrinkSidebar.UI.Views.EditConfigView
{
    /// <summary>管理一次同级拖拽的即时落点和提交，不计时、不移动其他节点。</summary>
    internal sealed class NodeReorderSession : IDisposable
    {
        private readonly System.Windows.Controls.Panel _panel;
        private readonly SubConfigView _source;
        private readonly FrameworkElement _sourceContainer;
        // 保存同级控件及其视图，提交时复用控件并更新只读序号。
        private readonly List<(FrameworkElement Container, SubConfigView View)> _items = new();
        private bool _disposed;
        private bool _insertAfter;

        /// <summary>当前有效目标，限定为同一父节点下的其他节点。</summary>
        internal SubConfigView? Target { get; private set; }

        /// <summary>记录现有同级控件，不创建快照或修改视觉布局。</summary>
        internal NodeReorderSession(SubConfigView source)
        {
            _source = source;
            _sourceContainer = source.DragContainer;
            _panel = (System.Windows.Controls.Panel)_sourceContainer.Parent;
            foreach (FrameworkElement container in _panel.Children)
            {
                var view = container as SubConfigView ?? (container as Border)?.Tag as SubConfigView;
                if (view == null)
                {
                    throw new InvalidOperationException("配置排序容器中存在无法识别的节点。");
                }
                _items.Add((container, view));
            }
        }

        /// <summary>立即按当前完整节点边界返回描边位置，滚动时重新转换坐标。</summary>
        internal Rect Update(FrameworkElement host, Point pointer, bool visible)
        {
            Target = null;
            if (_disposed || !visible)
            {
                return Rect.Empty;
            }
            foreach (var item in _items)
            {
                if (ReferenceEquals(item.View, _source))
                {
                    continue;
                }
                var bounds = new Rect(item.Container.TranslatePoint(new Point(), host), item.Container.RenderSize);
                if (bounds.Contains(pointer))
                {
                    Target = item.View;
                    _insertAfter = pointer.Y > bounds.Top + bounds.Height / 2;
                    return bounds;
                }
            }
            return Rect.Empty;
        }

        /// <summary>松手提交一次列表和控件移动，仅更新受影响同级节点的序号。</summary>
        internal void Commit(List<ButtonConfig> nodes)
        {
            if (_disposed || Target == null)
            {
                return;
            }
            var sourceIndex = nodes.IndexOf(_source.ConfigNode);
            var targetIndex = nodes.IndexOf(Target.ConfigNode);
            if (sourceIndex < 0 || targetIndex < 0)
            {
                return;
            }
            var index = targetIndex + (_insertAfter ? 1 : 0);
            if (sourceIndex < index)
            {
                index--;
            }
            Dispose();
            if (sourceIndex == index)
            {
                return;
            }
            nodes.RemoveAt(sourceIndex);
            nodes.Insert(index, _source.ConfigNode);
            // 同步移动同一个控件，保持滚动、展开和输入状态，不触发整树重建。
            _panel.Children.Remove(_sourceContainer);
            _panel.Children.Insert(index, _sourceContainer);
            foreach (var item in _items)
            {
                item.View.SiblingOrdinal = _panel.Children.IndexOf(item.Container) + 1;
            }
        }

        /// <summary>结束会话并清除候选，取消时没有数据或视觉布局需要回滚。</summary>
        public void Dispose()
        {
            Target = null;
            _disposed = true;
        }
    }
}
