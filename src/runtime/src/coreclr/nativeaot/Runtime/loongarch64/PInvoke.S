// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.


#include <unixasmmacros.inc>
#include "AsmOffsets.inc"

.global RhpTrapThreads

//
// RhpPInvoke
//
// IN:  a0: address of pinvoke frame
//
// This helper assumes that its callsite is as good to start the stackwalk as the actual PInvoke callsite.
// The codegenerator must treat the callsite of this helper as GC triggering and generate the GC info for it.
// Also, the codegenerator must ensure that there are no live GC references in callee saved registers.
//

NESTED_ENTRY RhpPInvoke, _TEXT, NoHandler
        st.d  $fp, $a0, OFFSETOF__PInvokeTransitionFrame__m_FramePointer
        st.d  $ra, $a0, OFFSETOF__PInvokeTransitionFrame__m_RIP
        st.d  $sp, $a0, OFFSETOF__PInvokeTransitionFrame__m_PreservedRegs
        ori  $t0, $zero, PTFF_SAVE_SP
        st.d  $t0, $a0, OFFSETOF__PInvokeTransitionFrame__m_Flags

        // get TLS global variable address

        INLINE_GETTHREAD  $a1

        st.d  $a1, $a0, OFFSETOF__PInvokeTransitionFrame__m_pThread
        st.d  $a0, $a1, OFFSETOF__Thread__m_pTransitionFrame
        jirl  $r0, $ra, 0
NESTED_END RhpPInvoke, _TEXT


LEAF_ENTRY RhpPInvokeReturn, _TEXT
        ld.d  $t0, $a0, OFFSETOF__PInvokeTransitionFrame__m_pThread
        st.d  $zero, $t0, OFFSETOF__Thread__m_pTransitionFrame

        PREPARE_EXTERNAL_VAR_INDIRECT_W RhpTrapThreads, $t0

        bnez  $t0, 0f  // TrapThreadsFlags_None = 0
        jirl  $r0, $ra, 0
0:
        // passing transition frame pointer in x0
        b  C_FUNC(RhpWaitForGC2)
LEAF_END RhpPInvokeReturn, _TEXT
