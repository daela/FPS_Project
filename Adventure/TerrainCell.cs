/*
	A terrain cell is a GridMap used to represent one subsection of the overworld's terrain.
*/
using System;
using Godot;

public class TerrainCell : GridMap{
	public int id; // id according to overworld.
	public Vector2 coords;

	public TerrainCell(){
		Theme = TerrainBlock.GetTheme();
		GD.Print("Theme: " + Theme);
		coords = new Vector2(-1, -1);
	}

	public TerrainCell(TerrainCellData data){
		Theme = TerrainBlock.GetTheme();
		GD.Print("Theme: " + Theme);
		LoadData(data);
	}

	public TerrainCellData GetData(){
		GD.Print("TerrainCell.GetData not implemented");
		coords = new Vector2();
		return null;
	}
	
	public void LoadData(TerrainCellData data){
		GD.Print("TerrainCell.LoadData not implemented");
		coords = data.coords;
		id = data.id;
		foreach(TerrainBlock block in data.blocks){
			int x = (int)block.gridPosition.x;
			int y = (int)block.gridPosition.y;
			int z = (int)block.gridPosition.z;
			int meshId = (int)block.blockId;

			SetCellItem(x, y, z, meshId);
		}
		
	}

}