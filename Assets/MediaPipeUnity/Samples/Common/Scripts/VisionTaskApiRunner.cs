// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections;
using Mediapipe.Tasks.Vision.Core;
using UnityEngine;

namespace Mediapipe.Unity.Sample
{
    public abstract class VisionTaskApiRunner<TTask> : BaseRunner where TTask : BaseVisionTaskApi
    {
        [SerializeField] protected Screen screen;

        public RunningMode runningMode;

        private Coroutine _coroutine;
        protected TTask taskApi;

        public override void Play()
        {
            if (_coroutine != null) Stop();
            base.Play();
            _coroutine = StartCoroutine(Run());
        }

        public override void Pause()
        {
            base.Pause();
            ImageSourceProvider.ImageSource.Pause();
        }

        public override void Resume()
        {
            base.Resume();
            var _ = StartCoroutine(ImageSourceProvider.ImageSource.Resume());
        }

        public override void Stop()
        {
            base.Stop();
            StopCoroutine(_coroutine);
            ImageSourceProvider.ImageSource.Stop();
            taskApi?.Close();
            taskApi = null;
        }

        protected abstract IEnumerator Run();

        protected static void SetupAnnotationController<T>(AnnotationController<T> annotationController,
            ImageSource imageSource, bool expectedToBeMirrored = false) where T : HierarchicalAnnotation
        {
            annotationController.isMirrored = expectedToBeMirrored;
            annotationController.imageSize = new Vector2Int(imageSource.textureWidth, imageSource.textureHeight);
        }
    }
}