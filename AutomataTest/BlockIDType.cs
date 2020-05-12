using System;

namespace AutomataTest
{
#define USE_USHORT_BLOCK_IDS

#if USE_USHORT_BLOCK_IDS
    using BLOCK_ID = UInt16;
#elif USE_UINT_BLOCK_IDS
    using BLOCK_ID = UInt32;
#elif USE_ULONG_BLOCK_IDS
    using BLOCK_ID = UInt64;
#endif
}
