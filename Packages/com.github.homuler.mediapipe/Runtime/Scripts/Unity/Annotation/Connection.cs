// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Mediapipe.Unity
{
    public class Connection
    {
        public readonly HierarchicalAnnotation end;
        public readonly HierarchicalAnnotation start;

        public Connection(HierarchicalAnnotation start, HierarchicalAnnotation end)
        {
            this.start = start;
            this.end = end;
        }
    }
}