using GenesisBestiary.Utilities;
using UnityEngine;

namespace GenesisBestiary.Player
{
    public enum PlayerState
    {
        Idle,
        Move,
        Attack,
        Dodge,
        Carve,
        Stagger,
        Dead
    }

    public sealed class PlayerStateMachine : StateMachine<PlayerState>
    {
        public PlayerStateMachine(PlayerController controller)
        {
            States[PlayerState.Idle] = new PlayerIdleState(this, controller);
            States[PlayerState.Move] = new PlayerMoveState(this, controller);
            ChangeState(PlayerState.Idle);
        }

        private sealed class PlayerIdleState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;

            public PlayerIdleState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
            }

            public void Tick()
            {
                if (controller.HasMoveInput)
                {
                    stateMachine.ChangeState(PlayerState.Move);
                }
            }

            public void Exit()
            {
            }
        }

        private sealed class PlayerMoveState : IState
        {
            private readonly PlayerStateMachine stateMachine;
            private readonly PlayerController controller;

            public PlayerMoveState(PlayerStateMachine stateMachine, PlayerController controller)
            {
                this.stateMachine = stateMachine;
                this.controller = controller;
            }

            public void Enter()
            {
            }

            public void Tick()
            {
                if (!controller.HasMoveInput)
                {
                    stateMachine.ChangeState(PlayerState.Idle);
                    return;
                }

                Vector3 moveDirection = controller.GetCameraRelativeMove();
                controller.Move(moveDirection);
                controller.RotateTowards(moveDirection);
            }

            public void Exit()
            {
            }
        }
    }
}
