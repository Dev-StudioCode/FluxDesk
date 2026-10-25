using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace FluxDesk
{
    public class VirtualDesktopInfo
    {
        public int Index { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string WallpaperPath { get; set; }
        public bool IsActive { get; set; }

        public string DisplayTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(Name))
                    return Name;
                return "Desktop " + (Index + 1);
            }
        }
    }

    public class VirtualDesktopService
    {
        private IVirtualDesktopManagerInternal _manager;
        private readonly object _lock = new object();

        public VirtualDesktopService()
        {
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                Type shellType = Type.GetTypeFromCLSID(Guids.CLSID_ImmersiveShell);
                IServiceProvider10 shell = (IServiceProvider10)Activator.CreateInstance(shellType);
                Guid managerGuid = typeof(IVirtualDesktopManagerInternal).GUID;
                Guid clsidVirtualDesktop = Guids.CLSID_VirtualDesktopManagerInternal;
                _manager = (IVirtualDesktopManagerInternal)shell.QueryService(ref clsidVirtualDesktop, ref managerGuid);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error initializing VirtualDesktopService: " + ex.Message);
            }
        }

        public int GetDesktopCount()
        {
            lock (_lock)
            {
                try
                {
                    if (_manager == null) Initialize();
                    return _manager != null ? _manager.GetCount() : 0;
                }
                catch
                {
                    Initialize();
                    return _manager != null ? _manager.GetCount() : 0;
                }
            }
        }

        public Guid GetCurrentDesktopId()
        {
            lock (_lock)
            {
                try
                {
                    if (_manager == null) Initialize();
                    if (_manager == null) return Guid.Empty;
                    IVirtualDesktop current = _manager.GetCurrentDesktop();
                    return current != null ? current.GetId() : Guid.Empty;
                }
                catch
                {
                    return Guid.Empty;
                }
            }
        }

        public List<VirtualDesktopInfo> GetAllDesktops()
        {
            lock (_lock)
            {
                var list = new List<VirtualDesktopInfo>();
                try
                {
                    if (_manager == null) Initialize();
                    if (_manager == null) return list;

                    int count = _manager.GetCount();
                    Guid currentId = GetCurrentDesktopId();

                    IObjectArray array;
                    _manager.GetDesktops(out array);
                    if (array == null) return list;

                    Guid vdGuid = typeof(IVirtualDesktop).GUID;
                    for (int i = 0; i < count; i++)
                    {
                        object obj;
                        array.GetAt(i, ref vdGuid, out obj);
                        IVirtualDesktop vd = obj as IVirtualDesktop;
                        if (vd != null)
                        {
                            Guid id = vd.GetId();
                            string name = vd.GetName();
                            string wp = vd.GetWallpaperPath();

                            list.Add(new VirtualDesktopInfo
                            {
                                Index = i,
                                Id = id,
                                Name = name,
                                WallpaperPath = wp,
                                IsActive = (id == currentId)
                            });
                        }
                    }
                    Marshal.ReleaseComObject(array);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("GetAllDesktops error: " + ex.Message);
                }
                return list;
            }
        }

        public bool SetDesktopWallpaper(int index, string wallpaperPath)
        {
            lock (_lock)
            {
                try
                {
                    if (_manager == null) Initialize();
                    if (_manager == null) return false;

                    int count = _manager.GetCount();
                    if (index < 0 || index >= count) return false;

                    IObjectArray array;
                    _manager.GetDesktops(out array);
                    if (array == null) return false;

                    Guid vdGuid = typeof(IVirtualDesktop).GUID;
                    object obj;
                    array.GetAt(index, ref vdGuid, out obj);
                    IVirtualDesktop vd = obj as IVirtualDesktop;
                    if (vd != null)
                    {
                        _manager.SetDesktopWallpaper(vd, wallpaperPath);
                    }
                    Marshal.ReleaseComObject(array);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SetDesktopWallpaper error for index " + index + ": " + ex.Message);
                    return false;
                }
            }
        }

        public bool SetDesktopWallpaperByGuid(Guid desktopId, string wallpaperPath)
        {
            lock (_lock)
            {
                try
                {
                    if (_manager == null) Initialize();
                    if (_manager == null) return false;

                    IVirtualDesktop vd = _manager.FindDesktop(ref desktopId);
                    if (vd != null)
                    {
                        _manager.SetDesktopWallpaper(vd, wallpaperPath);
                        return true;
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SetDesktopWallpaperByGuid error: " + ex.Message);
                    return false;
                }
            }
        }

        public void SwitchToDesktop(int index)
        {
            lock (_lock)
            {
                try
                {
                    if (_manager == null) Initialize();
                    if (_manager == null) return;

                    int count = _manager.GetCount();
                    if (index < 0 || index >= count) return;

                    IObjectArray array;
                    _manager.GetDesktops(out array);
                    if (array == null) return;

                    Guid vdGuid = typeof(IVirtualDesktop).GUID;
                    object obj;
                    array.GetAt(index, ref vdGuid, out obj);
                    IVirtualDesktop vd = obj as IVirtualDesktop;
                    if (vd != null)
                    {
                        _manager.SwitchDesktopWithAnimation(vd);
                    }
                    Marshal.ReleaseComObject(array);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SwitchToDesktop error: " + ex.Message);
                }
            }
        }
    }
}
