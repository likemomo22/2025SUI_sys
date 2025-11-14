// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Core;
using Mediapipe.Tasks.Vision.Core;
using Mediapipe.Tasks.Vision.GestureRecognizer.Proto;
using Mediapipe.Tasks.Vision.HandDetector.Proto;
using Mediapipe.Tasks.Vision.HandLandmarker.Proto;

namespace Mediapipe.Tasks.Vision.GestureRecognizer
{
  /// <summary>
  ///     Options for the gesture recognizer task.
  /// </summary>
  public sealed class GestureRecognizerOptions : ITaskOptions
    {
      /// <param name="gestureRecognizerResult">
      ///     The hand gesture recognition results.
      /// </param>
      /// <param name="image">
      ///     The input image that the gesture recognizer runs on.
      /// </param>
      /// <param name="timestampMillisec">
      ///     The input timestamp in milliseconds.
      /// </param>
      public delegate void ResultCallback(GestureRecognizerResult gestureRecognizerResult, Image image,
            long timestampMillisec);

        public GestureRecognizerOptions(
            BaseOptions baseOptions,
            RunningMode runningMode = RunningMode.IMAGE,
            int numHands = 1,
            float minHandDetectionConfidence = 0.5f,
            float minHandPresenceConfidence = 0.5f,
            float minTrackingConfidence = 0.5f,
            ClassifierOptions cannedGestureClassifierOptions = null,
            ClassifierOptions customGestureClassifierOptions = null,
            ResultCallback resultCallback = null)
        {
            this.baseOptions = baseOptions;
            this.runningMode = runningMode;
            this.numHands = numHands;
            this.minHandDetectionConfidence = minHandDetectionConfidence;
            this.minHandPresenceConfidence = minHandPresenceConfidence;
            this.minTrackingConfidence = minTrackingConfidence;
            this.cannedGestureClassifierOptions = cannedGestureClassifierOptions;
            this.customGestureClassifierOptions = customGestureClassifierOptions;
            this.resultCallback = resultCallback;
        }

        /// <summary>
        ///     Base options for the hand gesture recognizer task.
        /// </summary>
        public BaseOptions baseOptions { get; }

        /// <summary>
        ///     The running mode of the task. Default to the image mode.
        ///     GestureRecognizer has three running modes:
        ///     <list type="number">
        ///         <item>
        ///             <description>The image mode for recognizing hand gestures on single image inputs.</description>
        ///         </item>
        ///         <item>
        ///             <description>The video mode for recognizing hand gestures on the decoded frames of a video.</description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 The live stream mode for recognizing hand gestures on the live stream of input data, such as from
        ///                 camera.
        ///                 In this mode, the <see cref="resultCallback" /> below must be specified to receive the recognition
        ///                 results asynchronously.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </summary>
        public RunningMode runningMode { get; }

        /// <summary>
        ///     The maximum number of hands can be detected by the recognizer.
        /// </summary>
        public int numHands { get; }

        /// <summary>
        ///     The minimum confidence score for the hand detection to be considered successful.
        /// </summary>
        public float minHandDetectionConfidence { get; }

        /// <summary>
        ///     The minimum confidence score of hand presence score in the hand landmark detection.
        /// </summary>
        public float minHandPresenceConfidence { get; }

        /// <summary>
        ///     The minimum confidence score for the hand tracking to be considered successful.
        /// </summary>
        public float minTrackingConfidence { get; }

        /// <summary>
        ///     Options for configuring the canned
        ///     gestures classifier, such as score threshold, allow list and deny list of
        ///     gestures. The categories for canned gesture classifiers are: ["None",
        ///     "Closed_Fist", "Open_Palm", "Pointing_Up", "Thumb_Down", "Thumb_Up",
        ///     "Victory", "ILoveYou"]. Note this option is subject to change.
        /// </summary>
        public ClassifierOptions cannedGestureClassifierOptions { get; }

        /// <summary>
        ///     Options for configuring the custom
        ///     gestures classifier, such as score threshold, allow list and deny list of
        ///     gestures. Note this option is subject to change.
        /// </summary>
        public ClassifierOptions customGestureClassifierOptions { get; }

        /// <summary>
        ///     The user-defined result callback for processing live stream data.
        ///     The result callback should only be specified when the running mode is set to the live stream mode.
        /// </summary>
        public ResultCallback resultCallback { get; }

        CalculatorOptions ITaskOptions.ToCalculatorOptions()
        {
            var options = new CalculatorOptions();
            options.SetExtension(GestureRecognizerGraphOptions.Extensions.Ext, ToProto());
            return options;
        }

        internal GestureRecognizerGraphOptions ToProto()
        {
            var baseOptionsProto = baseOptions.ToProto();
            baseOptionsProto.UseStreamMode = runningMode != RunningMode.IMAGE;

            var handGestureRecognizerGraphOptions = new HandGestureRecognizerGraphOptions();
            if (cannedGestureClassifierOptions != null)
                handGestureRecognizerGraphOptions.CannedGestureClassifierGraphOptions =
                    new GestureClassifierGraphOptions
                    {
                        ClassifierOptions = cannedGestureClassifierOptions.ToProto()
                    };
            if (customGestureClassifierOptions != null)
                handGestureRecognizerGraphOptions.CustomGestureClassifierGraphOptions =
                    new GestureClassifierGraphOptions
                    {
                        ClassifierOptions = customGestureClassifierOptions.ToProto()
                    };

            return new GestureRecognizerGraphOptions
            {
                BaseOptions = baseOptionsProto,
                HandLandmarkerGraphOptions = new HandLandmarkerGraphOptions
                {
                    MinTrackingConfidence = minTrackingConfidence,
                    HandDetectorGraphOptions = new HandDetectorGraphOptions
                    {
                        NumHands = numHands,
                        MinDetectionConfidence = minHandDetectionConfidence
                    },
                    HandLandmarksDetectorGraphOptions = new HandLandmarksDetectorGraphOptions
                    {
                        MinDetectionConfidence = minHandPresenceConfidence
                    }
                },
                HandGestureRecognizerGraphOptions = handGestureRecognizerGraphOptions
            };
        }
    }
}