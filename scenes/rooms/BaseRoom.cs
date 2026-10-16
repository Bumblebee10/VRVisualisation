
using Godot;
using System.Collections.Generic;

public partial class BaseRoom : Node3D
{
	private GridMap _ceilings;
	private GridMap _floors;

	private readonly List<int> _cellIdsWithNoCeiling = new();

	public override void _Ready()
	{
		_ceilings = GetNode<GridMap>("%Ceilings");
		_floors = GetNode<GridMap>("%Floors");

		FillCeilings();
	}

	private void FillCeilings()
	{
		string[] cellNames =
		{
			"Ground",
			"Hole-Corner",
			"Hole-Side",
            "Hole-UTurn"
		};

		foreach (string cellName in cellNames)
		{
			_cellIdsWithNoCeiling.Add(
				_floors.MeshLibrary.FindItemByName(cellName)
			);
		}

		var usedCells = _floors.GetUsedCells();

		foreach (Vector3I cellCoords in usedCells)
		{
			int tileId = _floors.GetCellItem(cellCoords);

			if (_cellIdsWithNoCeiling.Contains(tileId))
			{
				_ceilings.SetCellItem(cellCoords, 0);
			}
		}
	}
}
