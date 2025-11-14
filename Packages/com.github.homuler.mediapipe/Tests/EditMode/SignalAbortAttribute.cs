// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System;
using NUnit.Framework;

namespace Mediapipe
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SignalAbortAttribute : CategoryAttribute
    {
    }
}