using System;
using System.Runtime.InteropServices;

namespace Helper {
  public class SmartGCHandle : IDisposable {
    private GCHandle handle;
    public SmartGCHandle(GCHandle handle) {
      this.handle = handle;
    }

    ~SmartGCHandle() {
      Dispose(false);
    }

    public System.IntPtr AddrOfPinnedObject() {
      return handle.AddrOfPinnedObject();
    }

    public virtual void Dispose() {
      Dispose(true);
    }

    protected virtual void Dispose(bool disposing) {
      this.handle.Free();
    }

    public static implicit operator GCHandle(SmartGCHandle other) {

      return other.handle;
    }
  }
}