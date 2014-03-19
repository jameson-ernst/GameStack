#pragma warning disable 0618

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using OpenTK;

#if __DESKTOP__
using OpenTK.Graphics.OpenGL;

#else
using OpenTK.Graphics.ES20;
#endif
namespace GameStack {
	public class Scene : IGameViewEventHandler, IDisposable {
		IGameViewEventSource _viewSource;
		List<IUpdater> _updaters;
		Dictionary<Type, List<Delegate>> _actions;
		Dictionary<Type, List<KeyValuePair<object,MethodInfo>>> _handlers;
		List<object> _unknowns;
		object[] _handlerArgs;

		public Scene () {
			this.ClearColor = Color.CornflowerBlue;

			_actions = new Dictionary<Type, List<Delegate>>();
			_updaters = new List<IUpdater>();
			_handlers = new Dictionary<Type, List<KeyValuePair<object, MethodInfo>>>();
			_handlerArgs = new object[2];
			_unknowns = new List<object>();

			this.Add(this);
		}

		public Scene (IGameViewEventSource viewSource)
			: this()
		{
			_viewSource = viewSource;
			_viewSource.Update += OnUpdate;
			_viewSource.Render += OnRender;
			_viewSource.Destroyed += OnDestroy;
		}

		public IGameViewEventSource ViewSource { get { return _viewSource; } }

		public Color ClearColor { get; set; }

		public void Add (object obj) {
			if (obj == null)
				throw new ArgumentNullException("Object must not be null.");

			var any = false;

			var updater = obj as IUpdater;
			if (updater != null) {
				_updaters.Add(updater);
				any = true;
			}

			var type = obj.GetType();
			foreach (var itype in type.GetInterfaces()) {
				if (itype.IsConstructedGenericType) {
					var gtype = itype.GetGenericTypeDefinition();
					if (gtype == typeof(IHandler<>)) {
						var atype = itype.GetGenericArguments()[0];
						var method = type.GetInterfaceMap(itype).TargetMethods[0];
						if (!_handlers.ContainsKey(atype))
							_handlers.Add(atype, new List<KeyValuePair<object,MethodInfo>>());
						_handlers[atype].Add(new KeyValuePair<object,MethodInfo>(obj, method));
						any = true;
					}
				}
			}

			if (!any)
				_unknowns.Add(obj);
		}

		public void Remove (object obj) {
			var any = false;

			var updater = obj as IUpdater;
			int idx;
			if (updater != null && (idx = _updaters.IndexOf(updater)) >= 0) {
				_updaters[idx] = null;
				any = true;
			}

			var type = obj.GetType();
			foreach (var itype in type.GetInterfaces()) {
				if (itype.IsConstructedGenericType) {
					var gtype = itype.GetGenericTypeDefinition();
					if (gtype == typeof(IHandler<>)) {
						var atype = itype.GetGenericArguments()[0];
						if (_handlers.ContainsKey(atype) && (idx = _handlers[atype].FindIndex(kv => kv.Key == obj)) >= 0) {
							var kv = _handlers[atype][idx];
							_handlers[atype][idx] = new KeyValuePair<object, MethodInfo>(obj, null);
						}
					}
				}
			}

			if (!any)
				_unknowns.Remove(obj);
		}

		public void ForEach<T> (Action<T> action) where T : class, IUpdater {
			for (var i = 0; i < _updaters.Count; i++) {
				var obj = _updaters[i] as T;
				if (obj != null)
					action(obj);
			}
		}

		public void AddHandler<T> (Action<FrameArgs, T> action) {
			var t = typeof(T);
			if (!_actions.ContainsKey(t))
				_actions.Add(t, new List<Delegate>());
			_actions[t].Add(action);
		}

		public void RemoveHandler<T> (Action<FrameArgs, T> action) {
			var t = typeof(T);
			int idx;
			if (_actions.ContainsKey(t) && (idx = _actions[t].IndexOf(action)) >= 0) {
				_actions[t][idx] = null;
			}
		}

		public void OnUpdate (object sender, FrameArgs e) {
			int count = 0;
			try {
				foreach (var evt in e.Events) {
					var t = evt.GetType();
					_handlerArgs[0] = e;
					_handlerArgs[1] = evt;

					List<Delegate> actionList;
					if (_actions.TryGetValue(t, out actionList)) {
						count = actionList.Count;
						for (var i = 0; i < count; i++) {
							if (actionList[i] != null)
								actionList[i].DynamicInvoke(_handlerArgs);
						}
					}

					List<KeyValuePair<object,MethodInfo>> handlerList;
					if (_handlers.TryGetValue(t, out handlerList)) {
						count = handlerList.Count;
						for (var i = 0; i < count; i++) {
							var kv = handlerList[i];
							if (kv.Value != null)
								kv.Value.Invoke(kv.Key, _handlerArgs);
						}
					}
				}
			}
			catch (TargetInvocationException ex) {
				if (ex.InnerException != null) {
					Console.WriteLine(ex.InnerException.ToString());
				}
				#if DEBUG
				throw;
				#endif
			}

			try {
				count = _updaters.Count;
				for (var i = 0; i < _updaters.Count; i++) {
					if (_updaters[i] != null)
						_updaters[i].Update(e);
				}
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				#if DEBUG
				throw;
				#endif
			}

			// cleanup
			foreach (var list in _actions.Values) {
				list.RemoveAll(o => o == null);
			}
			foreach (var list in _handlers.Values) {
				list.RemoveAll(o => o.Value == null);
			}
			_updaters.RemoveAll(o => o == null);

			e.ClearEvents();
		}

		public void OnDestroy (object sender, EventArgs e) {
			this.Dispose();
		}

		public void OnRender (object sender, FrameArgs e) {
			this.OnDraw(e);
		}

		protected virtual void OnDraw (FrameArgs e) {
#if __ANDROID__
            GL.Enable(All.DepthTest);
            GL.Enable(All.Blend);
            GL.BlendFunc(All.One, All.OneMinusSrcAlpha);
#else
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
#endif
			GL.DepthMask(true);
			GL.ClearColor(this.ClearColor);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
		}

		public virtual void Dispose () {
			if (_viewSource != null) {
				_viewSource.Update -= OnUpdate;
				_viewSource.Render -= OnRender;
				_viewSource.Destroyed -= OnDestroy;
			}
		}
	}
}

