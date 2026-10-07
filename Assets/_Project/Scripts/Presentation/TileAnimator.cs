using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game2048.Presentation
{
 
    public sealed class TileAnimator
    {
        private enum TweenKind
        {
            Move,
            Appear,
            Pop
        }

        private sealed class Tween
        {
            public TweenKind Kind;
            public RectTransform Target;
            public Vector2 From;
            public Vector2 To;
            public float Duration;
            public float Delay;
            public float Elapsed;
            public bool Started;
            public Action OnStart;
            public Action OnComplete;
        }

        private const float PopStrength = 0.15f;

        private readonly List<Tween> _active = new List<Tween>();
        private readonly List<Tween> _finished = new List<Tween>();

        public bool IsPlaying => _active.Count > 0;

        public void Move(RectTransform target, Vector2 to, float duration, Action onComplete = null)
        {
            _active.Add(new Tween
            {
                Kind = TweenKind.Move,
                Target = target,
                From = target.anchoredPosition,
                To = to,
                Duration = duration,
                OnComplete = onComplete
            });
        }

        public void Appear(RectTransform target, float delay, float duration)
        {
            target.localScale = Vector3.zero;
            _active.Add(new Tween { Kind = TweenKind.Appear, Target = target, Delay = delay, Duration = duration });
        }

        public void Pop(RectTransform target, float delay, float duration, Action onStart = null)
        {
            _active.Add(new Tween { Kind = TweenKind.Pop, Target = target, Delay = delay, Duration = duration, OnStart = onStart });
        }

        public void Tick(float deltaTime)
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                var tween = _active[i];
                tween.Elapsed += deltaTime;

                float local = tween.Elapsed - tween.Delay;
                if (local < 0f)
                    continue;

                StartIfNeeded(tween);
                float progress = tween.Duration <= 0f ? 1f : Mathf.Clamp01(local / tween.Duration);
                Apply(tween, progress);

                if (progress >= 1f)
                {
                    _active.RemoveAt(i);
                    _finished.Add(tween);
                }
            }

            FlushFinished();
        }

        
        public void CompleteAll()
        {
            while (_active.Count > 0)
            {
                _finished.AddRange(_active);
                _active.Clear();

                foreach (var tween in _finished)
                {
                    StartIfNeeded(tween);
                    Apply(tween, 1f);
                }

                FlushFinished();
            }
        }

        private void FlushFinished()
        {
            foreach (var tween in _finished)
                tween.OnComplete?.Invoke();
            _finished.Clear();
        }

        private static void StartIfNeeded(Tween tween)
        {
            if (tween.Started)
                return;

            tween.Started = true;
            tween.OnStart?.Invoke();
        }

        private static void Apply(Tween tween, float t)
        {
            switch (tween.Kind)
            {
                case TweenKind.Move:
                    tween.Target.anchoredPosition = Vector2.LerpUnclamped(tween.From, tween.To, EaseOutQuad(t));
                    break;
                case TweenKind.Appear:
                    tween.Target.localScale = Vector3.one * (t >= 1f ? 1f : EaseOutBack(t));
                    break;
                case TweenKind.Pop:
                    tween.Target.localScale = Vector3.one * (t >= 1f ? 1f : 1f + PopStrength * Mathf.Sin(t * Mathf.PI));
                    break;
            }
        }

        private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
