// Copyright (c) 2021 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;

using mplt = Mediapipe.LocationData.Types;
using mptcc = Mediapipe.Tasks.Components.Containers;

namespace Mediapipe.Unity
{
#pragma warning disable IDE0065
  using Color = UnityEngine.Color;
#pragma warning restore IDE0065

  public class PointListAnnotation : ListAnnotation<PointAnnotation>
  {
    [SerializeField] private Color _color = Color.green;
    [SerializeField] private float _radius = 15.0f;
    
    public GameObject targetMuscleCube ;
    public GameObject subMuscleCube1 ;
    public GameObject subMuscleCube2 ;
    
    void Awake()
    {
      if (targetMuscleCube == null)
      {
        targetMuscleCube = GameObject.Find("TargetMuscleCircle");
        Debug.Log("GET");
      }
      if (subMuscleCube1 == null)
      {
        subMuscleCube1 = GameObject.Find("SubMuscleCircle1");
      }
      if (subMuscleCube2 == null)
      {
        subMuscleCube2 = GameObject.Find("SubMuscleCircle2");
      }
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
      if (!UnityEditor.PrefabUtility.IsPartOfAnyPrefab(this))
      {
        ApplyColor(_color);
        ApplyRadius(_radius);
      }
    }
#endif

    public void SetColor(Color color)
    {
      _color = color;
      ApplyColor(_color);
    }

    public void SetRadius(float radius)
    {
      _radius = radius;
      ApplyRadius(_radius);
    }

    public void Draw(IReadOnlyList<Vector3> targets)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target); }
        });
      }
    }

    public void Draw(IReadOnlyList<Landmark> targets, Vector3 scale, bool visualizeZ = true)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, scale, visualizeZ); }
        });
      }
    }

    public void Draw(LandmarkList targets, Vector3 scale, bool visualizeZ = true)
    {
      Draw(targets.Landmark, scale, visualizeZ);
    }

    public void Draw(IReadOnlyList<NormalizedLandmark> targets, bool visualizeZ = true)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, visualizeZ); }
        });
      }
    }

    public void Draw(NormalizedLandmarkList targets, bool visualizeZ = true)
    {
      Draw(targets.Landmark, visualizeZ);
    }

    public void Draw(IReadOnlyList<mptcc.NormalizedLandmark> targets, bool visualizeZ = true)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null)
          {
            annotation.Draw(in target, visualizeZ);

            // 确保 landmarks 数足够
            if (children.Count > 15)
            {
              // 点11（右肩）
              targetMuscleCube.transform.position = children[11].transform.position;

              // 点9 和 点11 的中点
              var p9 = children[9].transform.position;
              var p11 = children[11].transform.position;
              subMuscleCube1.transform.position = (p9 + p11) / 2f;

              // 点13 和 点15 的中点（右臂中部）
              var p13 = children[13].transform.position;
              var p15 = children[15].transform.position;
              subMuscleCube2.transform.position = (p13 + p15) / 2f;
            }
            else
            {
              Debug.LogWarning($"Landmark 数量不足，当前 count = {children.Count}");
            }
          }
        });
      }
    }

    public void Draw(mptcc.NormalizedLandmarks targets, bool visualizeZ = true) => Draw(targets.landmarks, visualizeZ);

    public void Draw(IReadOnlyList<mplt.RelativeKeypoint> targets, float threshold = 0.0f)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, threshold); }
        });
      }
    }

    public void Draw(IReadOnlyList<mptcc.NormalizedKeypoint> targets, float threshold = 0.0f)
    {
      if (ActivateFor(targets))
      {
        CallActionForAll(targets, (annotation, target) =>
        {
          if (annotation != null) { annotation.Draw(target, threshold); }
        });
      }
    }

    protected override PointAnnotation InstantiateChild(bool isActive = true)
    {
      var annotation = base.InstantiateChild(isActive);
      annotation.SetColor(_color);
      annotation.SetRadius(_radius);
      return annotation;
    }

    private void ApplyColor(Color color)
    {
      foreach (var point in children)
      {
        if (point != null) { point.SetColor(color); }
      }
    }

    private void ApplyRadius(float radius)
    {
      foreach (var point in children)
      {
        if (point != null) { point.SetRadius(radius); }
      }
    }
  }
}
