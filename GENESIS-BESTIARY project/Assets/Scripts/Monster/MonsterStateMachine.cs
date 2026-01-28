using GenesisBestiary.Utilities;
using UnityEngine;

namespace GenesisBestiary.Monster
{
    public enum MonsterState
    {
        Idle,
        Roam,
        Chase,
        Attack,
        Flinch,
        Dead
    }

    public sealed class MonsterStateMachine : StateMachine<MonsterState>
    {
        private readonly MonsterController controller;
        
        public MonsterStateMachine(MonsterController controller)
        {
            this.controller = controller;
            
            States[MonsterState.Idle] = new IdleState(this, controller);
            States[MonsterState.Roam] = new RoamState(this, controller);
            States[MonsterState.Chase] = new ChaseState(this, controller);
            States[MonsterState.Attack] = new AttackState(this, controller);
            States[MonsterState.Flinch] = new FlinchState(this, controller);
            States[MonsterState.Dead] = new DeadState(this, controller);
            
            ChangeState(MonsterState.Idle);
        }

        public void RequestStateChange(MonsterState newState)
        {
            ChangeState(newState);
        }

        #region Idle State
        private sealed class IdleState : IState
        {
            private readonly MonsterStateMachine stateMachine;
            private readonly MonsterController controller;
            private float timer;

            public IdleState(MonsterStateMachine stateMachine, MonsterController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                timer = 0f;
                controller.StopMoving();
            }

            public void Tick()
            {
                // Check for player detection
                if (controller.CanSeePlayer())
                {
                    stateMachine.ChangeState(MonsterState.Chase);
                    return;
                }
                
                timer += Time.deltaTime;
                
                if (timer >= controller.IdleTime)
                {
                    stateMachine.ChangeState(MonsterState.Roam);
                }
            }

            public void Exit() { }
        }
        #endregion

        #region Roam State
        private sealed class RoamState : IState
        {
            private readonly MonsterStateMachine stateMachine;
            private readonly MonsterController controller;
            private Vector3 targetPosition;
            private bool hasTarget;

            public RoamState(MonsterStateMachine stateMachine, MonsterController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                targetPosition = controller.GetRandomRoamPosition();
                hasTarget = true;
            }

            public void Tick()
            {
                // Check for player detection
                if (controller.CanSeePlayer())
                {
                    stateMachine.ChangeState(MonsterState.Chase);
                    return;
                }
                
                if (!hasTarget)
                {
                    stateMachine.ChangeState(MonsterState.Idle);
                    return;
                }
                
                // Move towards target
                controller.MoveTowards(targetPosition, controller.MoveSpeed);
                
                // Check if reached target
                float distance = Vector3.Distance(controller.transform.position, targetPosition);
                if (distance < 1f)
                {
                    hasTarget = false;
                    stateMachine.ChangeState(MonsterState.Idle);
                }
            }

            public void Exit() { }
        }
        #endregion

        #region Chase State
        private sealed class ChaseState : IState
        {
            private readonly MonsterStateMachine stateMachine;
            private readonly MonsterController controller;

            public ChaseState(MonsterStateMachine stateMachine, MonsterController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter() { }

            public void Tick()
            {
                Transform player = controller.GetPlayerTransform();
                if (player == null)
                {
                    stateMachine.ChangeState(MonsterState.Idle);
                    return;
                }
                
                float distance = Vector3.Distance(controller.transform.position, player.position);
                
                // In attack range?
                if (distance <= controller.AttackRange)
                {
                    stateMachine.ChangeState(MonsterState.Attack);
                    return;
                }
                
                // Lost sight of player?
                if (distance > controller.DetectionRange * 1.5f)
                {
                    stateMachine.ChangeState(MonsterState.Idle);
                    return;
                }
                
                // Chase player
                controller.MoveTowards(player.position, controller.ChaseSpeed);
            }

            public void Exit() { }
        }
        #endregion

        #region Attack State
        private sealed class AttackState : IState
        {
            private readonly MonsterStateMachine stateMachine;
            private readonly MonsterController controller;
            private float attackTimer;
            private bool hasDealtDamage;
            private MonsterAttackData currentAttack;
            private bool useDefaultAttack;

            public AttackState(MonsterStateMachine stateMachine, MonsterController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                attackTimer = 0f;
                hasDealtDamage = false;
                currentAttack = controller.SelectAttack();
                useDefaultAttack = (currentAttack == null);
                controller.StopMoving();
                
                // Face the player
                Transform player = controller.GetPlayerTransform();
                if (player != null)
                {
                    controller.FaceTarget(player.position);
                }
            }

            public void Tick()
            {
                attackTimer += Time.deltaTime;
                
                if (useDefaultAttack)
                {
                    // デフォルト攻撃を使用
                    float startup = controller.GetDefaultAttackStartup();
                    
                    if (!hasDealtDamage && attackTimer >= startup)
                    {
                        controller.PerformDefaultAttackHitDetection();
                        hasDealtDamage = true;
                    }
                    
                    if (attackTimer >= controller.DefaultAttackTotalDuration)
                    {
                        stateMachine.ChangeState(MonsterState.Chase);
                    }
                }
                else
                {
                    // MonsterAttackDataを使用
                    if (!hasDealtDamage && attackTimer >= currentAttack.startup)
                    {
                        controller.PerformAttackHitDetection(currentAttack);
                        hasDealtDamage = true;
                    }
                    
                    if (attackTimer >= currentAttack.TotalDuration)
                    {
                        stateMachine.ChangeState(MonsterState.Chase);
                    }
                }
            }

            public void Exit() { }
        }
        #endregion

        #region Flinch State
        private sealed class FlinchState : IState
        {
            private readonly MonsterStateMachine stateMachine;
            private readonly MonsterController controller;
            private float timer;
            private const float FLINCH_DURATION = 1f;

            public FlinchState(MonsterStateMachine stateMachine, MonsterController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                timer = 0f;
                controller.StopMoving();
                controller.ResetFlinch();
            }

            public void Tick()
            {
                timer += Time.deltaTime;
                
                if (timer >= FLINCH_DURATION)
                {
                    stateMachine.ChangeState(MonsterState.Chase);
                }
            }

            public void Exit() { }
        }
        #endregion

        #region Dead State
        private sealed class DeadState : IState
        {
            private readonly MonsterStateMachine stateMachine;
            private readonly MonsterController controller;

            public DeadState(MonsterStateMachine stateMachine, MonsterController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
                controller.StopMoving();
                controller.OnDeath();
            }

            public void Tick() { }

            public void Exit() { }
        }
        #endregion
    }
}
