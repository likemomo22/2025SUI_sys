// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Core.Proto;
using NUnit.Framework;
using BaseOptions = Mediapipe.Tasks.Core.BaseOptions;

namespace Mediapipe.Tests.Tasks.Core
{
    public class BaseOptionsTest
    {
        #region ToProto

        [TestCase(BaseOptions.Delegate.CPU, Acceleration.DelegateOneofCase.Tflite)]
        [TestCase(BaseOptions.Delegate.GPU, Acceleration.DelegateOneofCase.Gpu)]
        public void ToProto_ShouldSetDelegateCase(BaseOptions.Delegate delegateCase,
            Acceleration.DelegateOneofCase delegateOneofCase)
        {
            var options = new BaseOptions(delegateCase);
            var proto = options.ToProto();
            Assert.AreEqual(delegateOneofCase, proto.Acceleration.DelegateCase);
        }

        [Test]
        public void ToProto_ShouldSetModelAsset_When_ModelAssetPathIsSet()
        {
            var path = "asset/path";
            var options = new BaseOptions(modelAssetPath: path);
            var proto = options.ToProto();
            Assert.AreEqual(path, proto.ModelAsset.FileName);
        }

        [Test]
        public void ToProto_ShouldSetModelAsset_When_ModelAssetBufferIsSet()
        {
            var contents = new byte[] { 1, 2, 3 };
            var options = new BaseOptions(modelAssetBuffer: contents);
            var proto = options.ToProto();
            Assert.AreEqual(contents, proto.ModelAsset.FileContent.ToByteArray());
        }

        #endregion
    }
}