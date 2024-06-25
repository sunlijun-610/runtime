﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace System.Numerics.Tensors
{
    public static unsafe partial class TensorPrimitives
    {
        /// <summary>Operator that takes one input value and returns a single value.</summary>
        /// <remarks>The input type must be a quarter of the size of the output type.</remarks>
        private interface IUnaryOneToFourOperator<TInput, TOutput>
        {
            static abstract bool Vectorizable { get; }
            static abstract TOutput Invoke(TInput x);
            static abstract (Vector128<TOutput>, Vector128<TOutput>, Vector128<TOutput>, Vector128<TOutput>) Invoke(Vector128<TInput> x);
            static abstract (Vector256<TOutput>, Vector256<TOutput>, Vector256<TOutput>, Vector256<TOutput>) Invoke(Vector256<TInput> x);
            static abstract (Vector512<TOutput>, Vector512<TOutput>, Vector512<TOutput>, Vector512<TOutput>) Invoke(Vector512<TInput> x);
        }

        /// <summary>Performs an element-wise operation on <paramref name="x"/> and writes the results to <paramref name="destination"/>.</summary>
        /// <typeparam name="TInput">The element input type.</typeparam>
        /// <typeparam name="TOutput">The element output type. Must be four times the size of TInput.</typeparam>
        /// <typeparam name="TUnaryOperator">Specifies the operation to perform on each element loaded from <paramref name="x"/>.</typeparam>
        /// <remarks>This should only be used when it's known that TInput/TOutput are vectorizable and the size of TInput is a fourth that of TOutput.</remarks>
        private static void InvokeSpanIntoSpan_1to4<TInput, TOutput, TUnaryOperator>(
            ReadOnlySpan<TInput> x, Span<TOutput> destination)
            where TUnaryOperator : struct, IUnaryOneToFourOperator<TInput, TOutput>
        {
            Debug.Assert(sizeof(TInput) * 4 == sizeof(TOutput));

            if (x.Length > destination.Length)
            {
                ThrowHelper.ThrowArgument_DestinationTooShort();
            }

            ref TInput sourceRef = ref MemoryMarshal.GetReference(x);
            ref TOutput destinationRef = ref MemoryMarshal.GetReference(destination);
            int i = 0, oneVectorFromEnd;

            if (Vector512.IsHardwareAccelerated && TUnaryOperator.Vectorizable)
            {
                Debug.Assert(Vector512<TInput>.IsSupported);
                Debug.Assert(Vector512<TOutput>.IsSupported);

                oneVectorFromEnd = x.Length - Vector512<TInput>.Count;
                if (i <= oneVectorFromEnd)
                {
                    // Loop handling one input vector / four output vectors at a time.
                    do
                    {
                        (Vector512<TOutput>, Vector512<TOutput>, Vector512<TOutput>, Vector512<TOutput>) results = TUnaryOperator.Invoke(Vector512.LoadUnsafe(ref sourceRef, (uint)i));
                        results.Item1.StoreUnsafe(ref destinationRef, (uint)i);
                        results.Item2.StoreUnsafe(ref destinationRef, (uint)(i + Vector512<TOutput>.Count));
                        results.Item3.StoreUnsafe(ref destinationRef, (uint)(i + (Vector512<TOutput>.Count * 2)));
                        results.Item4.StoreUnsafe(ref destinationRef, (uint)(i + (Vector512<TOutput>.Count * 3)));

                        i += Vector512<TInput>.Count;
                    }
                    while (i <= oneVectorFromEnd);

                    // Handle any remaining elements with a final input vector.
                    if (i != x.Length)
                    {
                        i = x.Length - Vector512<TInput>.Count;

                        (Vector512<TOutput>, Vector512<TOutput>, Vector512<TOutput>, Vector512<TOutput>) results = TUnaryOperator.Invoke(Vector512.LoadUnsafe(ref sourceRef, (uint)i));
                        results.Item1.StoreUnsafe(ref destinationRef, (uint)i);
                        results.Item2.StoreUnsafe(ref destinationRef, (uint)(i + Vector512<TOutput>.Count));
                        results.Item3.StoreUnsafe(ref destinationRef, (uint)(i + (Vector512<TOutput>.Count * 2)));
                        results.Item4.StoreUnsafe(ref destinationRef, (uint)(i + (Vector512<TOutput>.Count * 3)));
                    }

                    return;
                }
            }

            if (Vector256.IsHardwareAccelerated && TUnaryOperator.Vectorizable)
            {
                Debug.Assert(Vector256<TInput>.IsSupported);
                Debug.Assert(Vector256<TOutput>.IsSupported);

                oneVectorFromEnd = x.Length - Vector256<TInput>.Count;
                if (i <= oneVectorFromEnd)
                {
                    // Loop handling one input vector / two output vectors at a time.
                    do
                    {
                        (Vector256<TOutput>, Vector256<TOutput>, Vector256<TOutput>, Vector256<TOutput>) results = TUnaryOperator.Invoke(Vector256.LoadUnsafe(ref sourceRef, (uint)i));
                        results.Item1.StoreUnsafe(ref destinationRef, (uint)i);
                        results.Item2.StoreUnsafe(ref destinationRef, (uint)(i + Vector256<TOutput>.Count));
                        results.Item3.StoreUnsafe(ref destinationRef, (uint)(i + (Vector256<TOutput>.Count * 2)));
                        results.Item4.StoreUnsafe(ref destinationRef, (uint)(i + (Vector256<TOutput>.Count * 3)));

                        i += Vector256<TInput>.Count;
                    }
                    while (i <= oneVectorFromEnd);

                    // Handle any remaining elements with a final input vector.
                    if (i != x.Length)
                    {
                        i = x.Length - Vector256<TInput>.Count;

                        (Vector256<TOutput>, Vector256<TOutput>, Vector256<TOutput>, Vector256<TOutput>) results = TUnaryOperator.Invoke(Vector256.LoadUnsafe(ref sourceRef, (uint)i));
                        results.Item1.StoreUnsafe(ref destinationRef, (uint)i);
                        results.Item2.StoreUnsafe(ref destinationRef, (uint)(i + Vector256<TOutput>.Count));
                        results.Item3.StoreUnsafe(ref destinationRef, (uint)(i + (Vector256<TOutput>.Count * 2)));
                        results.Item4.StoreUnsafe(ref destinationRef, (uint)(i + (Vector256<TOutput>.Count * 3)));
                    }

                    return;
                }
            }

            if (Vector128.IsHardwareAccelerated && TUnaryOperator.Vectorizable)
            {
                Debug.Assert(Vector128<TInput>.IsSupported);
                Debug.Assert(Vector128<TOutput>.IsSupported);

                oneVectorFromEnd = x.Length - Vector128<TInput>.Count;
                if (i <= oneVectorFromEnd)
                {
                    // Loop handling one input vector / two output vectors at a time.
                    do
                    {
                        (Vector128<TOutput>, Vector128<TOutput>, Vector128<TOutput>, Vector128<TOutput>) results = TUnaryOperator.Invoke(Vector128.LoadUnsafe(ref sourceRef, (uint)i));
                        results.Item1.StoreUnsafe(ref destinationRef, (uint)i);
                        results.Item2.StoreUnsafe(ref destinationRef, (uint)(i + Vector128<TOutput>.Count));
                        results.Item3.StoreUnsafe(ref destinationRef, (uint)(i + (Vector128<TOutput>.Count * 2)));
                        results.Item4.StoreUnsafe(ref destinationRef, (uint)(i + (Vector128<TOutput>.Count * 3)));

                        i += Vector128<TInput>.Count;
                    }
                    while (i <= oneVectorFromEnd);

                    // Handle any remaining elements with a final input vector.
                    if (i != x.Length)
                    {
                        i = x.Length - Vector128<TInput>.Count;

                        (Vector128<TOutput>, Vector128<TOutput>, Vector128<TOutput>, Vector128<TOutput>) results = TUnaryOperator.Invoke(Vector128.LoadUnsafe(ref sourceRef, (uint)i));
                        results.Item1.StoreUnsafe(ref destinationRef, (uint)i);
                        results.Item2.StoreUnsafe(ref destinationRef, (uint)(i + Vector128<TOutput>.Count));
                        results.Item3.StoreUnsafe(ref destinationRef, (uint)(i + (Vector128<TOutput>.Count * 2)));
                        results.Item4.StoreUnsafe(ref destinationRef, (uint)(i + (Vector128<TOutput>.Count * 3)));
                    }

                    return;
                }
            }

            while (i < x.Length)
            {
                Unsafe.Add(ref destinationRef, i) = TUnaryOperator.Invoke(Unsafe.Add(ref sourceRef, i));
                i++;
            }
        }
    }
}
