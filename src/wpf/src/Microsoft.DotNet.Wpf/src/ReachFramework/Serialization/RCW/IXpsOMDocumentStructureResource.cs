// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace System.Windows.Xps.Serialization.RCW
{
    /// <summary>
    /// RCW for xpsobjectmodel.idl found in Windows SDK
    /// This is generated code with minor manual edits. 
    /// i.  Generate TLB
    ///      MIDL /TLB xpsobjectmodel.tlb xpsobjectmodel.IDL //xpsobjectmodel.IDL found in Windows SDK
    /// ii. Generate RCW in a DLL
    ///      TLBIMP xpsobjectmodel.tlb // Generates xpsobjectmodel.dll
    /// iii.Decompile the DLL and copy out the RCW by hand.
    ///      ILDASM xpsobjectmodel.dll
    /// </summary>

    [Guid("85FEBC8A-6B63-48A9-AF07-7064E4ECFF30"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    internal interface IXpsOMDocumentStructureResource : IXpsOMResource
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.Interface)]
        new IOpcPartUri GetPartName();

        [MethodImpl(MethodImplOptions.InternalCall)]
        new void SetPartName([MarshalAs(UnmanagedType.Interface)] [In] IOpcPartUri partUri);

        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.Interface)]
        IXpsOMDocument GetOwner();

        [MethodImpl(MethodImplOptions.InternalCall)]
        [return: MarshalAs(UnmanagedType.Interface)]
        IStream GetStream();

        [MethodImpl(MethodImplOptions.InternalCall)]
        void SetContent([MarshalAs(UnmanagedType.Interface)] [In] IStream sourceStream, [MarshalAs(UnmanagedType.Interface)] [In] IOpcPartUri partName);
    }
}
