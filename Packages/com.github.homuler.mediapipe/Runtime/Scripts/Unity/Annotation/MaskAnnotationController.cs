// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

namespace Mediapipe.Unity
{
    public class MaskAnnotationController : AnnotationController<MaskAnnotation>
    {
        private ImageFrame _currentTarget;
        private int _maskHeight;
        private int _maskWidth;

        public void InitScreen(int maskWidth, int maskHeight)
        {
            _maskWidth = maskWidth;
            _maskHeight = maskHeight;
            annotation.Init(_maskWidth, _maskHeight);
        }

        public void DrawNow(ImageFrame target)
        {
            _currentTarget = target;
            UpdateMaskArray(_currentTarget);
            SyncNow();
        }

        public void DrawLater(ImageFrame target)
        {
            UpdateCurrentTarget(target, ref _currentTarget);
            UpdateMaskArray(_currentTarget);
        }

        private void UpdateMaskArray(ImageFrame imageFrame)
        {
            if (imageFrame != null) annotation.Read(imageFrame);
        }

        protected override void SyncNow()
        {
            isStale = false;
            if (_currentTarget == null)
                annotation.Clear();
            else
                annotation.Draw();
        }
    }
}