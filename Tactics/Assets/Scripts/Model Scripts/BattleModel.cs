using UnityEngine;

namespace Battle
{
    public class BattleModel : MonoBehaviour
    {
        [field: SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public BattleModel GhostModel { get; private set; }
        private bool IsGhost => GhostModel == null;

        public Vector3 ConvertOrientation(Tile.Direction direction)
        {
            return direction switch
            {
                Tile.Direction.Top => Vector3.forward,
                Tile.Direction.Right => Vector3.right,
                Tile.Direction.Down => Vector3.back,    
                Tile.Direction.Left => Vector3.left,
                Tile.Direction.TopRight => (Vector3.forward + Vector3.right).normalized,
                Tile.Direction.DownRight => (Vector3.back + Vector3.right).normalized,
                Tile.Direction.DownLeft => (Vector3.back + Vector3.left).normalized,
                Tile.Direction.TopLeft => (Vector3.forward + Vector3.left).normalized,
                _ => Vector3.forward
            };
        }

        public void SetOrientation(Tile.Direction direction)
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