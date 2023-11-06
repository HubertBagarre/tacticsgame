using UnityEngine;

namespace Battle
{
    public class BattleModel : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public BattleModel GhostModel { get; private set; }
        private bool IsGhost => GhostModel == null;

        public Vector3 ConvertOrientation(NewTile.Direction direction)
        {
            return direction switch
            {
                NewTile.Direction.Top => Vector3.forward,
                NewTile.Direction.Right => Vector3.right,
                NewTile.Direction.Down => Vector3.back,    
                NewTile.Direction.Left => Vector3.left,
                NewTile.Direction.TopRight => (Vector3.forward + Vector3.right).normalized,
                NewTile.Direction.DownRight => (Vector3.back + Vector3.right).normalized,
                NewTile.Direction.DownLeft => (Vector3.back + Vector3.left).normalized,
                NewTile.Direction.TopLeft => (Vector3.forward + Vector3.left).normalized,
                _ => Vector3.forward
            };
        }

        public void SetOrientation(NewTile.Direction direction)
        {
            SetOrientation(ConvertOrientation(direction));
        }

        public void SetOrientation(Vector3 direction)
        {
            transform.forward = direction.normalized;
            
            if(IsGhost) return;
            GhostModel.SetOrientation(direction.normalized);
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
            
            if(IsGhost) return;
            GhostModel.SetPosition(position);
        }

        public void ShowGhost(bool value)
        {
            if(IsGhost) return;
            GhostModel.gameObject.SetActive(value);
        }
    }
}