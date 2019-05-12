using System;
using System.Collections.Generic;
using System.IO;
using uTinyRipper.Assembly.Mono;
using uTinyRipper.Exporters.Scripts;

namespace uTinyRipper.Assembly
{
	public sealed class AssemblyManager : IAssemblyManager
	{
		public AssemblyManager(Action<string> requestAssemblyCallback)
		{
			if (requestAssemblyCallback == null)
			{
				throw new ArgumentNullException(nameof(requestAssemblyCallback));
			}
			m_requestAssemblyCallback = requestAssemblyCallback;
		}

		~AssemblyManager()
		{
			Dispose(false);
		}

		public static bool IsAssembly(string fileName)
		{
			if (MonoManager.IsMonoAssembly(fileName))
			{
				return true;
			}
			return false;
		}

		public static string ToAssemblyName(string scopeName)
		{
			if (scopeName.EndsWith(MonoManager.AssemblyExtension))
			{
				return scopeName.Substring(0, scopeName.Length - MonoManager.AssemblyExtension.Length);
			}
			return scopeName;
		}

		public void Load(string filePath)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			m_manager.Load(filePath);
		}

		public void Read(Stream stream, string fileName)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			m_manager.Read(stream, fileName);
		}

		public void Unload(string fileName)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				throw new Exception("You have to set backend first");
			}
			m_manager.Unload(fileName);
		}

		public bool IsAssemblyLoaded(string assembly)
		{
			return m_manager.IsAssemblyLoaded(assembly);
		}

		public bool IsPresent(string assembly, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return false;
			}
			return m_manager.IsPresent(assembly, name);
		}

		public bool IsPresent(string assembly, string @namespace, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return false;
			}
			return m_manager.IsPresent(assembly, @namespace, name);
		}

		public bool IsValid(string assembly, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return false;
			}
			return m_manager.IsValid(assembly, name);
		}

		public bool IsValid(string assembly, string @namespace, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return false;
			}
			return m_manager.IsValid(assembly, @namespace, name);
		}

		public ScriptStructure CreateStructure(string assembly, string name)
		{
			return m_manager.CreateStructure(assembly, name);
		}

		public ScriptStructure CreateStructure(string assembly, string @namespace, string name)
		{
			return m_manager.CreateStructure(assembly, @namespace, name);
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string name)
		{
			return m_manager.CreateExportType(exportManager, assembly, name);
		}

		public ScriptExportType CreateExportType(ScriptExportManager exportManager, string assembly, string @namespace, string name)
		{
			return m_manager.CreateExportType(exportManager, assembly, @namespace, name);
		}

		public ScriptInfo GetScriptInfo(string assembly, string name)
		{
			if (ScriptingBackEnd == ScriptingBackEnd.Unknown)
			{
				return default;
			}
			return m_manager.GetScriptInfo(assembly, name);
		}

		internal void InvokeRequestAssemblyCallback(string assemblyName)
		{
			m_requestAssemblyCallback.Invoke(assemblyName);
		}

		internal void AddSerializableType(string uniqueName, ScriptType scriptType)
		{
			m_serializableTypes.Add(uniqueName, scriptType);
		}

		internal bool TryGetSerializableType(string uniqueName, out ScriptType scriptType)
		{
			return m_serializableTypes.TryGetValue(uniqueName, out scriptType);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (m_manager != null)
			{
				m_manager.Dispose();
			}
		}

		public ScriptingBackEnd ScriptingBackEnd
		{
			get => m_manager == null ? ScriptingBackEnd.Unknown : m_manager.ScriptingBackEnd;
			set
			{
				if (ScriptingBackEnd == value)
				{
					return;
				}

				switch (value)
				{
					case ScriptingBackEnd.Mono:
						m_manager = new MonoManager(this);
						break;

					default:
						throw new NotImplementedException($"Scripting backend {value} is not impelented yet");
				}
			}
		}

		private event Action<string> m_requestAssemblyCallback;

		private IAssemblyManager m_manager;
		private Dictionary<string, ScriptType> m_serializableTypes = new Dictionary<string, ScriptType>();

	}
}
