using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "어떤 적을, 어떤 대열로, 몇 마리, 어디서, 언제" 소환할지를 적은 한 줄짜리 소환 명령.
/// EnemyManager가 이 목록을 순서대로 실행한다. Inspector에서 자유롭게 수정/추가할 수 있다.
/// </summary>
[Serializable]
public class WaveGroup
{
    [Header("What")]
    public EnemyKind enemyKind = EnemyKind.Scout;
    public FormationType formation = FormationType.Line;
    [Min(1)] public int count = 5;

    [Header("Where")]
    [Tooltip("대열 안에서 적 사이의 간격(월드 단위)")]
    [Min(0.3f)] public float spacing = 1.2f;
    [Tooltip("대열 중심의 가로 위치. -1 = 화면 왼쪽 끝, 0 = 중앙, 1 = 오른쪽 끝")]
    [Range(-1f, 1f)] public float spawnX = 0f;
    [Tooltip("Grid 대열일 때 한 줄에 몇 마리를 둘지")]
    [Min(1)] public int columns = 5;

    [Header("When")]
    [Tooltip("이전 그룹 소환이 끝난 뒤 기다리는 시간(초)")]
    [Min(0f)] public float delayBefore = 2f;
    [Tooltip("0이면 대열 전체가 동시에 나오고, 0보다 크면 한 마리씩 간격을 두고 나온다")]
    [Min(0f)] public float spawnInterval = 0f;

    public WaveGroup() { }

    public WaveGroup(EnemyKind kind, FormationType formation, int count, float spacing,
                     float spawnX, float delayBefore, int columns = 5, float spawnInterval = 0f)
    {
        this.enemyKind = kind;
        this.formation = formation;
        this.count = count;
        this.spacing = spacing;
        this.spawnX = spawnX;
        this.delayBefore = delayBefore;
        this.columns = columns;
        this.spawnInterval = spawnInterval;
    }

    /// <summary>
    /// 기본 스테이지 구성 (총 9개 그룹, 7가지 대열 모두 등장).
    /// EnemyManager 컴포넌트를 처음 붙일 때 Inspector에 이 값이 채워져 있고, 이후 Inspector에서 수정할 수 있다.
    /// </summary>
    public static List<WaveGroup> CreateDefaultWaves()
    {
        return new List<WaveGroup>
        {
            //          종류               대열                  수   간격  x     대기  열
            new WaveGroup(EnemyKind.Scout,  FormationType.Line,      5, 1.3f,  0f,  1.5f),
            new WaveGroup(EnemyKind.Scout,  FormationType.V,         7, 0.9f,  0f,  3.0f),
            new WaveGroup(EnemyKind.Weaver, FormationType.Zigzag,    8, 1.0f,  0f,  3.0f),
            new WaveGroup(EnemyKind.Gunner, FormationType.Line,      3, 2.2f,  0f,  4.0f),
            new WaveGroup(EnemyKind.Scout,  FormationType.Circle,   10, 0.9f,  0f,  4.0f),
            new WaveGroup(EnemyKind.Chaser, FormationType.InvertedV, 5, 1.0f,  0f,  4.0f),
            new WaveGroup(EnemyKind.Weaver, FormationType.Column,    6, 1.0f, -0.7f, 3.0f, 5, 0.35f),
            new WaveGroup(EnemyKind.Gunner, FormationType.Grid,      6, 1.6f,  0f,  4.0f, 3),
            new WaveGroup(EnemyKind.Chaser, FormationType.Line,      4, 1.4f,  0f,  2.0f),
        };
    }
}
