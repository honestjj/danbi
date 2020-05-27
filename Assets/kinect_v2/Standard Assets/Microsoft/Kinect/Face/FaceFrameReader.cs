using System.Collections.Generic;

using RootSystem = System;
namespace Microsoft.Kinect.Face {
  //
  // Microsoft.Kinect.Face.FaceFrameReader
  //
  public sealed partial class FaceFrameReader : RootSystem.IDisposable, Helper.INativeWrapper {
    internal RootSystem.IntPtr _pNative;
    RootSystem.IntPtr Helper.INativeWrapper.nativePtr { get { return _pNative; } }

    // Constructors and Finalizers
    internal FaceFrameReader(RootSystem.IntPtr pNative) {
      _pNative = pNative;
      Microsoft_Kinect_Face_FaceFrameReader_AddRefObject(ref _pNative);
    }

    ~FaceFrameReader() {
      Dispose(false);
    }

    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern void Microsoft_Kinect_Face_FaceFrameReader_ReleaseObject(ref RootSystem.IntPtr pNative);
    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern void Microsoft_Kinect_Face_FaceFrameReader_AddRefObject(ref RootSystem.IntPtr pNative);
    private void Dispose(bool disposing) {
      if (_pNative == RootSystem.IntPtr.Zero) {
        return;
      }

      __EventCleanup();

      Helper.NativeObjectCache.RemoveObject<FaceFrameReader>(_pNative);

      if (disposing) {
        Microsoft_Kinect_Face_FaceFrameReader_Dispose(_pNative);
      }
      Microsoft_Kinect_Face_FaceFrameReader_ReleaseObject(ref _pNative);

      _pNative = RootSystem.IntPtr.Zero;
    }


    // Public Properties
    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern bool Microsoft_Kinect_Face_FaceFrameReader_get_IsPaused(RootSystem.IntPtr pNative);
    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern void Microsoft_Kinect_Face_FaceFrameReader_put_IsPaused(RootSystem.IntPtr pNative, bool isPaused);
    public bool IsPaused {
      get {
        if (_pNative == RootSystem.IntPtr.Zero) {
          throw new RootSystem.ObjectDisposedException("FaceFrameReader");
        }

        return Microsoft_Kinect_Face_FaceFrameReader_get_IsPaused(_pNative);
      }
      set {
        if (_pNative == RootSystem.IntPtr.Zero) {
          throw new RootSystem.ObjectDisposedException("FaceFrameReader");
        }

        Microsoft_Kinect_Face_FaceFrameReader_put_IsPaused(_pNative, value);
        Helper.ExceptionHelper.CheckLastError();
      }
    }

    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern RootSystem.IntPtr Microsoft_Kinect_Face_FaceFrameReader_get_FaceFrameSource(RootSystem.IntPtr pNative);
    public Microsoft.Kinect.Face.FaceFrameSource FaceFrameSource {
      get {
        if (_pNative == RootSystem.IntPtr.Zero) {
          throw new RootSystem.ObjectDisposedException("FaceFrameReader");
        }

        RootSystem.IntPtr objectPointer = Microsoft_Kinect_Face_FaceFrameReader_get_FaceFrameSource(_pNative);
        Helper.ExceptionHelper.CheckLastError();
        if (objectPointer == RootSystem.IntPtr.Zero) {
          return null;
        }

        return Helper.NativeObjectCache.CreateOrGetObject<Microsoft.Kinect.Face.FaceFrameSource>(objectPointer, n => new Microsoft.Kinect.Face.FaceFrameSource(n));
      }
    }


    // Events
    private static RootSystem.Runtime.InteropServices.GCHandle _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handle;
    [RootSystem.Runtime.InteropServices.UnmanagedFunctionPointer(RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
    private delegate void _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate(RootSystem.IntPtr args, RootSystem.IntPtr pNative);
    private static Helper.CollectionMap<RootSystem.IntPtr, List<RootSystem.EventHandler<Microsoft.Kinect.Face.FaceFrameArrivedEventArgs>>> Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks = new Helper.CollectionMap<RootSystem.IntPtr, List<RootSystem.EventHandler<Microsoft.Kinect.Face.FaceFrameArrivedEventArgs>>>();
    [AOT.MonoPInvokeCallbackAttribute(typeof(_Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate))]
    private static void Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handler(RootSystem.IntPtr result, RootSystem.IntPtr pNative) {
      List<RootSystem.EventHandler<Microsoft.Kinect.Face.FaceFrameArrivedEventArgs>> callbackList = null;
      Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks.TryGetValue(pNative, out callbackList);
      lock (callbackList) {
        var objThis = Helper.NativeObjectCache.GetObject<FaceFrameReader>(pNative);
        var args = new Microsoft.Kinect.Face.FaceFrameArrivedEventArgs(result);
        foreach (var func in callbackList) {
          Helper.EventPump.Instance.Enqueue(() => { try { func(objThis, args); } catch { } });
        }
      }
    }
    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern void Microsoft_Kinect_Face_FaceFrameReader_add_FrameArrived(RootSystem.IntPtr pNative, _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate eventCallback, bool unsubscribe);
    public event RootSystem.EventHandler<Microsoft.Kinect.Face.FaceFrameArrivedEventArgs> FrameArrived {
      add {
        Helper.EventPump.EnsureInitialized();

        Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks.TryAddDefault(_pNative);
        var callbackList = Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks[_pNative];
        lock (callbackList) {
          callbackList.Add(value);
          if (callbackList.Count == 1) {
            var del = new _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate(Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handler);
            _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handle = RootSystem.Runtime.InteropServices.GCHandle.Alloc(del);
            Microsoft_Kinect_Face_FaceFrameReader_add_FrameArrived(_pNative, del, false);
          }
        }
      }
      remove {
        if (_pNative == RootSystem.IntPtr.Zero) {
          return;
        }

        Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks.TryAddDefault(_pNative);
        var callbackList = Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks[_pNative];
        lock (callbackList) {
          callbackList.Remove(value);
          if (callbackList.Count == 0) {
            Microsoft_Kinect_Face_FaceFrameReader_add_FrameArrived(_pNative, Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handler, true);
            _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handle.Free();
          }
        }
      }
    }

    private static RootSystem.Runtime.InteropServices.GCHandle _Windows_Data_PropertyChangedEventArgs_Delegate_Handle;
    [RootSystem.Runtime.InteropServices.UnmanagedFunctionPointer(RootSystem.Runtime.InteropServices.CallingConvention.Cdecl)]
    private delegate void _Windows_Data_PropertyChangedEventArgs_Delegate(RootSystem.IntPtr args, RootSystem.IntPtr pNative);
    private static Helper.CollectionMap<RootSystem.IntPtr, List<RootSystem.EventHandler<Windows.Data.PropertyChangedEventArgs>>> Windows_Data_PropertyChangedEventArgs_Delegate_callbacks = new Helper.CollectionMap<RootSystem.IntPtr, List<RootSystem.EventHandler<Windows.Data.PropertyChangedEventArgs>>>();
    [AOT.MonoPInvokeCallbackAttribute(typeof(_Windows_Data_PropertyChangedEventArgs_Delegate))]
    private static void Windows_Data_PropertyChangedEventArgs_Delegate_Handler(RootSystem.IntPtr result, RootSystem.IntPtr pNative) {
      List<RootSystem.EventHandler<Windows.Data.PropertyChangedEventArgs>> callbackList = null;
      Windows_Data_PropertyChangedEventArgs_Delegate_callbacks.TryGetValue(pNative, out callbackList);
      lock (callbackList) {
        var objThis = Helper.NativeObjectCache.GetObject<FaceFrameReader>(pNative);
        var args = new Windows.Data.PropertyChangedEventArgs(result);
        foreach (var func in callbackList) {
          Helper.EventPump.Instance.Enqueue(() => { try { func(objThis, args); } catch { } });
        }
      }
    }
    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern void Microsoft_Kinect_Face_FaceFrameReader_add_PropertyChanged(RootSystem.IntPtr pNative, _Windows_Data_PropertyChangedEventArgs_Delegate eventCallback, bool unsubscribe);
    public event RootSystem.EventHandler<Windows.Data.PropertyChangedEventArgs> PropertyChanged {
      add {
        Helper.EventPump.EnsureInitialized();

        Windows_Data_PropertyChangedEventArgs_Delegate_callbacks.TryAddDefault(_pNative);
        var callbackList = Windows_Data_PropertyChangedEventArgs_Delegate_callbacks[_pNative];
        lock (callbackList) {
          callbackList.Add(value);
          if (callbackList.Count == 1) {
            var del = new _Windows_Data_PropertyChangedEventArgs_Delegate(Windows_Data_PropertyChangedEventArgs_Delegate_Handler);
            _Windows_Data_PropertyChangedEventArgs_Delegate_Handle = RootSystem.Runtime.InteropServices.GCHandle.Alloc(del);
            Microsoft_Kinect_Face_FaceFrameReader_add_PropertyChanged(_pNative, del, false);
          }
        }
      }
      remove {
        if (_pNative == RootSystem.IntPtr.Zero) {
          return;
        }

        Windows_Data_PropertyChangedEventArgs_Delegate_callbacks.TryAddDefault(_pNative);
        var callbackList = Windows_Data_PropertyChangedEventArgs_Delegate_callbacks[_pNative];
        lock (callbackList) {
          callbackList.Remove(value);
          if (callbackList.Count == 0) {
            Microsoft_Kinect_Face_FaceFrameReader_add_PropertyChanged(_pNative, Windows_Data_PropertyChangedEventArgs_Delegate_Handler, true);
            _Windows_Data_PropertyChangedEventArgs_Delegate_Handle.Free();
          }
        }
      }
    }


    // Public Methods
    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern RootSystem.IntPtr Microsoft_Kinect_Face_FaceFrameReader_AcquireLatestFrame(RootSystem.IntPtr pNative);
    public Microsoft.Kinect.Face.FaceFrame AcquireLatestFrame() {
      if (_pNative == RootSystem.IntPtr.Zero) {
        throw new RootSystem.ObjectDisposedException("FaceFrameReader");
      }

      RootSystem.IntPtr objectPointer = Microsoft_Kinect_Face_FaceFrameReader_AcquireLatestFrame(_pNative);
      Helper.ExceptionHelper.CheckLastError();
      if (objectPointer == RootSystem.IntPtr.Zero) {
        return null;
      }

      return Helper.NativeObjectCache.CreateOrGetObject<Microsoft.Kinect.Face.FaceFrame>(objectPointer, n => new Microsoft.Kinect.Face.FaceFrame(n));
    }

    [RootSystem.Runtime.InteropServices.DllImport("KinectFaceUnityAddin", CallingConvention = RootSystem.Runtime.InteropServices.CallingConvention.Cdecl, SetLastError = true)]
    private static extern void Microsoft_Kinect_Face_FaceFrameReader_Dispose(RootSystem.IntPtr pNative);
    public void Dispose() {
      if (_pNative == RootSystem.IntPtr.Zero) {
        return;
      }

      Dispose(true);
      RootSystem.GC.SuppressFinalize(this);
    }

    private void __EventCleanup() {
      {
        Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks.TryAddDefault(_pNative);
        var callbackList = Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_callbacks[_pNative];
        lock (callbackList) {
          if (callbackList.Count > 0) {
            callbackList.Clear();
            if (_pNative != RootSystem.IntPtr.Zero) {
              Microsoft_Kinect_Face_FaceFrameReader_add_FrameArrived(_pNative, Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handler, true);
            }
            _Microsoft_Kinect_Face_FaceFrameArrivedEventArgs_Delegate_Handle.Free();
          }
        }
      }
      {
        Windows_Data_PropertyChangedEventArgs_Delegate_callbacks.TryAddDefault(_pNative);
        var callbackList = Windows_Data_PropertyChangedEventArgs_Delegate_callbacks[_pNative];
        lock (callbackList) {
          if (callbackList.Count > 0) {
            callbackList.Clear();
            if (_pNative != RootSystem.IntPtr.Zero) {
              Microsoft_Kinect_Face_FaceFrameReader_add_PropertyChanged(_pNative, Windows_Data_PropertyChangedEventArgs_Delegate_Handler, true);
            }
            _Windows_Data_PropertyChangedEventArgs_Delegate_Handle.Free();
          }
        }
      }
    }
  }

}
