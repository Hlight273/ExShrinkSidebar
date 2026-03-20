using System;
using System.Collections.Generic;

namespace ExShrinkSidebar.Script.Core.Event
{
    /// <summary>
    /// 事件参数基类
    /// </summary>
    public class EventBase : EventArgs
    {
        public string EventId { get; set; }
        public object Sender { get; set; }
    }

    /// <summary>
    /// 泛型事件参数
    /// </summary>
    public class EventBase<T> : EventBase
    {
        public T Data { get; set; }
    }

    /// <summary>
    /// 事件处理器委托
    /// </summary>
    public delegate void EventHandler<in T>(object sender, T args) where T : EventBase;

    /// <summary>
    /// 全局事件管理器
    /// </summary>
    public static class EVENT
    {
        // 存储事件处理器
        private static readonly Dictionary<string, List<Delegate>> _handlers = new Dictionary<string, List<Delegate>>();

        /// <summary>
        /// 注册事件监听
        /// </summary>
        public static void on<T>(string eventId, EventHandler<T> handler) where T : EventBase
        {
            if (string.IsNullOrEmpty(eventId)) throw new ArgumentNullException(nameof(eventId));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_handlers)
            {
                if (!_handlers.ContainsKey(eventId))
                {
                    _handlers[eventId] = new List<Delegate>();
                }
                _handlers[eventId].Add(handler);
            }
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        public static void emit<T>(string eventId, object sender, T data) where T : EventBase
        {
            if (string.IsNullOrEmpty(eventId)) throw new ArgumentNullException(nameof(eventId));

            List<Delegate> handlers = null;
            lock (_handlers)
            {
                if (_handlers.ContainsKey(eventId))
                {
                    handlers = new List<Delegate>(_handlers[eventId]);
                }
            }

            if (handlers == null) return;

            data.EventId = eventId;
            data.Sender = sender;

            foreach (var handler in handlers)
            {
                try
                {
                    (handler as EventHandler<T>)?.Invoke(sender, data);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"事件执行错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 触发事件（简化版，不指定发送者）
        /// </summary>
        public static void emit<T>(string eventId, T data) where T : EventBase
        {
            emit(eventId, null, data);
        }

        /// <summary>
        /// 移除特定的事件处理器
        /// </summary>
        public static void off<T>(string eventId, EventHandler<T> handler) where T : EventBase
        {
            if (string.IsNullOrEmpty(eventId)) throw new ArgumentNullException(nameof(eventId));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            lock (_handlers)
            {
                if (_handlers.ContainsKey(eventId))
                {
                    _handlers[eventId].Remove(handler);
                    if (_handlers[eventId].Count == 0)
                    {
                        _handlers.Remove(eventId);
                    }
                }
            }
        }

        /// <summary>
        /// 移除指定事件的所有处理器
        /// </summary>
        public static void off(string eventId)
        {
            if (string.IsNullOrEmpty(eventId)) throw new ArgumentNullException(nameof(eventId));

            lock (_handlers)
            {
                _handlers.Remove(eventId);
            }
        }

        /// <summary>
        /// 移除指定目标对象的所有事件处理器
        /// </summary>
        public static void targetOff(object target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));

            lock (_handlers)
            {
                var keysToRemove = new List<string>();

                foreach (var kvp in _handlers)
                {
                    kvp.Value.RemoveAll(d =>
                    {
                        try { return d.Target == target; }
                        catch { return false; }
                    });

                    if (kvp.Value.Count == 0)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in keysToRemove)
                {
                    _handlers.Remove(key);
                }
            }
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public static void clear()
        {
            lock (_handlers)
            {
                _handlers.Clear();
            }
        }
    }
}