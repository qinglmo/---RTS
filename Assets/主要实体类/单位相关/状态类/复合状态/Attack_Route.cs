using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 推进状态，有路线的攻击
/// </summary>
public class Attack_Route : BaseBranchState
{
    //实现细节，复合状态进入时，重新计算目标点路线，包含攻击子状态，追击子状态,返回路线子状态，沿着路线前进子状态。转换逻辑由决策树实现。
    //需要注意的数据，路线列表，返回距离（离开路线多远后触发返回），追击范围特化（避免追击过远目标），到达距离（距离终点多远算到达，视线优化，看到目标才算到达）
    //关于路线完成，怎么算到达路线的下一个格子呢？不一定需要单位完全到达格子，只需要单位距离下一个格子的距离比当前格子的距离近，就认为到达下一个格子了。
    public Attack_Route(BaseUnit unit) : base(unit) {}
    private List<Vector2Int> routeList;
    private int currentIndex = 0;
    public Vector2Int NewRoutePoint { get { return routeList[currentIndex]; } }
    public override void Enter()
    {
        stateMachine = new StateMachine(stateEnum =>
        {
            return stateEnum switch
            {

                UnitState.Attacking => unit is EnemyUnit ? new AttackState(unit as EnemyUnit) : new ShootAttackState(unit as Unit),
                UnitState.Chaseing => new ApproachState(unit, stateMachine),
                UnitState.TargetAdvancement => new RouteAdvancement(unit,this),//特定状态，属于当前状态的专属子状态，耦合较深。
                UnitState.ReturnRoute => new ApproachPosState(unit,stateMachine),

                _ => throw new ArgumentException("无效状态")
            };
        }, unit, 2);
        stateMachine.Initialize(UnitState.TargetAdvancement);
        AddRules();
        unit.chaseRange_Current = unit.chaseRange_Advance;//追击范围特化，避免追击过远目标
        routeList = Pathfinding.FindPath_OneTarget(unit.AttackTarget.Value, unit,MovementRules.IsWalkable_Static);//获取到达目标点静态路线，该路径会在推进状态持续存在，执行其他任务后会重置
        currentIndex = 0;
        if(routeList==null||routeList.Count==0)
        {
            unit.mainState.StateMachine.ChangeState(UnitState.Top_Attack);
        }
        Debug.LogWarning($"推进任务节点，路线长度：{routeList.Count}");
    }
    public override void Reset()
    {
        base.Reset();
        unit.chaseRange_Current = unit.chaseRange_Advance;//追击范围特化，避免追击过远目标
        routeList = Pathfinding.FindPath_OneTarget(unit.AttackTarget.Value, unit,MovementRules.IsWalkable_Static);//获取到达目标点静态路线，该路径会在推进状态持续存在，执行其他任务后会重置
        if(routeList==null||routeList.Count==0)
        {
            unit.mainState.StateMachine.ChangeState(UnitState.Top_Attack);
        }
        currentIndex = 0;
        Debug.LogWarning($"推进任务节点，路线长度：{routeList.Count}");
    }
    public override void Update()
    {
        // 1. 先更路线推进：不管你在干嘛，路点的进度都要更
        UpdateRoutePoint();
        if (routeList.Count - currentIndex <= 3)//终点检查
        {
            unit.AttackTarget = null;
            unit.mainState.StateMachine.ChangeState(UnitState.Top_Attack);
        }
        if(DistanceCalculate.Heuristic(unit.GridPos, NewRoutePoint)>=6)//强制返回
        {
            StateMachine.ChangeState(UnitState.ReturnRoute);
        }
        // 2. 再跑子状态机的更新
       stateMachine.Update();
    }       
    private void AddRules()
    {
        //攻击状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.AttackTargetFound, ToState = UnitState.Attacking });
        //追击状态转入,禁用返回转入
        stateMachine.AddRule(new TransitionRule { FromState=UnitState.Attacking, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        stateMachine.AddRule(new TransitionRule { FromState=UnitState.TargetAdvancement, TriggerEvent = StateEvent.ChaseTargetFound, ToState = UnitState.Chaseing });
        //返回状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.OverRange, ToState = UnitState.ReturnRoute });
        //推进状态转入
        stateMachine.AddRule(new TransitionRule { fromStateHasValue = false, TriggerEvent = StateEvent.Idle, ToState = UnitState.TargetAdvancement });
    }
    private void UpdateRoutePoint()
    {
        if(routeList==null||routeList.Count==0)
        {
            return;
        }
        if (currentIndex == routeList.Count-1)
        {
            return;
        }
        if(DistanceCalculate.Heuristic(unit.GridPos, NewRoutePoint)>= DistanceCalculate.Heuristic(unit.GridPos, routeList[currentIndex + 1]))
        {
            currentIndex++;
        }
    }

    //实际体验来讲，推进状态只会在没有敌人的时候触发，所以不太需要考虑有没有敌人的问题。
    public class RouteAdvancement : IState
    {
        //关于终点检查，有外部决策树处理，这里只负责推进路线。所以通常来说不太可能没有剩余路线。

        //检查路线的下一个格子，如果有友军就寻找移动代价为2的替代格子，只要能更新当前路点即可，没有就扩大范围，直到超过一个设置，比如6格。
        //还可以注意到，其实当前位置不一定在路线上，所以移动到下一个格子的代价很可能一开始就很高，这样时候遵循先接近格子为先，然后再考虑移动代价。
        //需要注意纪律性，只考虑下一个格子，不考虑下下个格子，下下个格子应该在到达下一个格子后考虑。避免过度思考，反而混乱。
        private BaseUnit unit;
        private Attack_Route attack_Route;
        private bool isWait=false;//是否正在等待期间，等待期间每0.5s检查是否可以前往下一个路点。
        private float waitTime=0.5f;
        private float Timer=0;
        public RouteAdvancement(BaseUnit unit,Attack_Route attack_Route) { this.unit = unit; this.attack_Route=attack_Route; }
        public void Enter()
        {
            isWait = false;
            Timer = 0;
        }

        public void Exit()
        {
        }

        public void Reset()
        {
            Enter();
        }

        public void Update()
        {
            if(isWait)
            {
                Timer+=Time.deltaTime;
                if(Timer>=waitTime)
                {
                    isWait=false;
                    Timer=0;
                }
                return;
            }
            if(unit.movement.IsMoving) return;
            
            if (attack_Route.currentIndex == attack_Route.routeList.Count-1)
            {
                return;
            }
            if (unit.GridPos == attack_Route.NewRoutePoint)//在路线中，正常推进路线
            {
                //新路径包括下一个格子和后续所有格子。
                List<Vector2Int> nextPath=new List<Vector2Int>(attack_Route.routeList.GetRange(attack_Route.currentIndex+1,attack_Route.routeList.Count-attack_Route.currentIndex-1));
                unit.movement.MoveToGridWithPaths(nextPath.Last(),nextPath);//实际上这个会处理空路径
                isWait=true;
                Timer=0;
                return;
            }
            else
            {
                unit.targetPosition=attack_Route.NewRoutePoint;
                attack_Route.stateMachine.ChangeState(UnitState.ReturnRoute);
            }

        }
        /// <summary>
        /// 寻找满足【路点推进条件】的下一个移动格子（BFS 保证最近、代价最优）
        /// 规则：
        /// 1. 从 currentPos 开始广度优先搜索
        /// 2. 格子必须在 currentRoutePoint / nextRoutePoint 周围曼哈顿距离 ≤6 内
        /// 3. 必须满足：格子到 nextRoutePoint < 格子到 currentRoutePoint
        /// 4. 返回第一个满足条件的格子（最近、代价最低）
        /// 5. 无满足条件 → 返回 null 表示无效
        /// </summary>
        private List<Vector2Int> FindNextRoutePoint(Vector2Int currentPos,Vector2Int currentRoutePoint,Vector2Int nextRoutePoint)
        {
            
            // 实际上有两种考虑，一种是先接近，一种是先更新。先接近时指尽可能接近下一个路点的同时花费最少的移动代价。先更新是指只要能更新路点，且花费移动代价最少即可
            //出于简单考虑，先采用先更新的策略。所以可能出现单位在路径边缘移动的情况，不过这个影响不大，先把功能跑通吧。
            // 搜索方向：上下左右（四方向BFS，移动代价最低）
            Vector2Int[] dirs = new Vector2Int[]
            {
                new(0, 1), new(0, -1), new(1, 0), new(-1, 0)
            };

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            queue.Enqueue(currentPos);
            visited.Add(currentPos);
            cameFrom[currentPos] = currentPos;

            const int maxRange = 6;
            Vector2Int? foundTarget = null;

            while (queue.Count > 0)
            {
                Vector2Int checkPos = queue.Dequeue();

                int disToCur = DistanceCalculate.Heuristic(checkPos, currentRoutePoint);
                int disToNext = DistanceCalculate.Heuristic(checkPos, nextRoutePoint);

                // 超出双路点范围 → 跳过
                if (disToCur > maxRange && disToNext > maxRange)
                    continue;

                // 不可行走 → 跳过
                if (!MovementRules.IsWalkable(checkPos, unit))
                    continue;

                // ======================
                // 满足条件：找到目标点！
                // ======================
                if (MovementRules.IsPreOccupyable(checkPos, unit) && disToNext < disToCur)
                {
                    foundTarget = checkPos;
                    break;
                }

                // 扩展
                foreach (var dir in dirs)
                {
                    Vector2Int neighbor = checkPos + dir;
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        cameFrom[neighbor] = checkPos;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // 没找到
            if (!foundTarget.HasValue)
                return null;

            // ======================
            // 回溯生成完整路径
            // ======================
            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int cur = foundTarget.Value;

            while (cur != currentPos)
            {
                path.Add(cur);
                cur = cameFrom[cur];
            }

            // 反转路径 → 从起点到终点
            path.Reverse();
            return path;
        }
    }
}