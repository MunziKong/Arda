using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Try Random Order",
    story: "Try children in random order",
    category: "Flow",
    id: "b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9")]
public partial class TryRandomOrderComposite : Composite
{
    private readonly List<int> _order = new();
    private int _currentIndex;

    protected override Status OnStart()
    {
        if (Children == null || Children.Count == 0)
        {
            return Status.Failure;
        }

        _order.Clear();
        for (int i = 0; i < Children.Count; i++)
        {
            _order.Add(i);
        }

        // Fisher-Yates shuffle
        for (int i = _order.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (_order[i], _order[j]) = (_order[j], _order[i]);
        }

        _currentIndex = 0;
        return TryStartNext();
    }

    protected override Status OnUpdate()
    {
        if (_currentIndex >= _order.Count)
        {
            return Status.Failure;
        }

        var childStatus = Children[_order[_currentIndex]].CurrentStatus;

        if (childStatus == Status.Running)
        {
            return Status.Running;
        }

        if (childStatus == Status.Success)
        {
            return Status.Success;
        }

        // 현재 자식 실패 → 다음으로
        _currentIndex++;
        return TryStartNext();
    }

    protected override void OnEnd()
    {
        if (_currentIndex < _order.Count)
        {
            var child = Children[_order[_currentIndex]];
            if (child.CurrentStatus == Status.Running)
            {
                EndNode(child);
            }
        }
    }

    private Status TryStartNext()
    {
        while (_currentIndex < _order.Count)
        {
            Status result = StartNode(Children[_order[_currentIndex]]);

            if (result == Status.Success) { return Status.Success; }
            if (result == Status.Running) { return Status.Running; }

            // 즉시 Failure(거리 밖, 쿨타임 등) → 다음 자식으로
            _currentIndex++;
        }

        // 모든 자식 실패 → Chase로 넘어가도록 Failure
        return Status.Failure;
    }
}
