// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Google.Protobuf;
using Mediapipe.Tasks.Core.Proto;

namespace Mediapipe.Tasks.Core
{
    public sealed class BaseOptions
    {
        public enum Delegate
        {
            CPU = 0,
            GPU = 1,

            // Edge TPU acceleration using NNAPI delegate.
            EDGETPU_NNAPI = 2
        }

        public BaseOptions(Delegate delegateCase = Delegate.CPU, string modelAssetPath = null,
            byte[] modelAssetBuffer = null)
        {
            this.delegateCase = delegateCase;
            this.modelAssetPath = modelAssetPath;
            this.modelAssetBuffer = modelAssetBuffer;
        }

        public Delegate delegateCase { get; } = Delegate.CPU;
        public string modelAssetPath { get; } = string.Empty;
        public byte[] modelAssetBuffer { get; }

        private Acceleration acceleration
        {
            get
            {
                switch (delegateCase)
                {
                    case Delegate.CPU:
                        return new Acceleration
                        {
                            Tflite = new InferenceCalculatorOptions.Types.Delegate.Types.TfLite()
                        };
                    case Delegate.GPU:
                        return new Acceleration
                        {
                            Gpu = new InferenceCalculatorOptions.Types.Delegate.Types.Gpu
                            {
                                UseAdvancedGpuApi = true
                            }
                        };
                    case Delegate.EDGETPU_NNAPI:
                        return new Acceleration
                        {
                            Nnapi = new InferenceCalculatorOptions.Types.Delegate.Types.Nnapi
                            {
                                AcceleratorName = "google-edgetpu"
                            }
                        };
                    default:
                        return null;
                }
            }
        }

        private ExternalFile modelAsset
        {
            get
            {
                var file = new ExternalFile();

                if (modelAssetPath != null) file.FileName = modelAssetPath;
                if (modelAssetBuffer != null) file.FileContent = ByteString.CopyFrom(modelAssetBuffer);

                return file;
            }
        }

        internal Proto.BaseOptions ToProto()
        {
            return new Proto.BaseOptions
            {
                ModelAsset = modelAsset,
                Acceleration = acceleration
            };
        }
    }
}