////
////  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
////  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
////  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
////  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
////  REMAINS UNCHANGED.
////
////  Email:  gustavo_franco@hotmail.com
////
////  Copyright (C) 2006 Franco, Gustavo 
////
//#define DEBUGON

//using System;
//using System.Text;
//using System.Threading;
//using System.Collections;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;
//using Microsoft.Xna.Framework;
//using TWD;

//namespace Algorithms
//{
//    public class PathFinder
//    {
//        private int width;
//        private int height;
//        private Cell[,] Cells;
//        Point startCell;
//        Point endCell;

//        /// <summary>
//        /// Create a new instance of PathFinder
//        /// </summary>
//        /// <param name="searchParameters"></param>
//        public PathFinder(Cell[,] Cells, Point start, Point end)
//        {
//            this.Cells = Cells;
//            this.startCell = start;
//            this.Cells[start.X, start.Y].Estado = Cell.TipoEstado.Aberto;
//            this.endCell = end;
//        }

//        /// <summary>
//        /// Attempts to find a path from the start location to the end location based on the supplied SearchParameters
//        /// </summary>
//        /// <returns>A List of Points representing the path. If no path was found, the returned list is empty.</returns>
//        public List<Point> FindPath()
//        {
//            // The start Cell is the first entry in the 'open' list
//            List<Point> path = new List<Point>();
//            bool success = Search(startCell);
//            if (success)
//            {
//                // If a path was found, follow the parents from the end Cell to build a list of locations
//                Cell Cell = this.endCell;
//                while (Cell.ParentCell != null)
//                {
//                    path.Add(Cell.Location);
//                    Cell = Cell.ParentCell;
//                }

//                // Reverse the list so it's in the correct order when returned
//                path.Reverse();
//            }

//            return path;
//        }

//        /// <summary>
//        /// Builds the Cell grid from a simple grid of booleans indicating areas which are and aren't walkable
//        /// </summary>
//        /// <param name="map">A boolean representation of a grid in which true = walkable and false = not walkable</param>
//        private void InitializeCells(bool[,] map)
//        {
//            this.width = map.GetLength(0);
//            this.height = map.GetLength(1);
//            this.Cells = new Cell[this.width, this.height];
//            for (int y = 0; y < this.height; y++)
//            {
//                for (int x = 0; x < this.width; x++)
//                {
//                    this.Cells[x, y] = new Cell(x, y, map[x, y], this.searchParameters.EndLocation);
//                }
//            }
//        }

//        /// <summary>
//        /// Attempts to find a path to the destination Cell using <paramref name="currentCell"/> as the starting location
//        /// </summary>
//        /// <param name="currentCell">The Cell from which to find a path</param>
//        /// <returns>True if a path to the destination has been found, otherwise false</returns>
//        private bool Search(Cell currentCell)
//        {
//            // Set the current Cell to Closed since it cannot be traversed more than once
//            currentCell.Estado = Cell.TipoEstado.Fechado;
//            List<Cell> nextCells = GetAdjacentWalkableCells(currentCell);

//            // Sort by F-value so that the shortest possible routes are considered first
//            nextCells.Sort((Cell1, Cell2) => Cell1.F.CompareTo(Cell2.F));
//            foreach (var nextCell in nextCells)
//            {
//                // Check whether the end Cell has been reached
//                if (nextCell.Location == this.endCell.Location)
//                {
//                    return true;
//                }
//                else
//                {
//                    // If not, check the next set of Cells
//                    if (Search(nextCell)) // Note: Recurses back into Search(Cell)
//                        return true;
//                }
//            }

//            // The method returns false if this path leads to be a dead end
//            return false;
//        }

//        /// <summary>
//        /// Returns any Cells that are adjacent to <paramref name="fromCell"/> and may be considered to form the next step in the path
//        /// </summary>
//        /// <param name="fromCell">The Cell from which to return the next possible Cells in the path</param>
//        /// <returns>A list of next possible Cells in the path</returns>
//        private List<Cell> GetAdjacentWalkableCells(Cell fromCell)
//        {
//            List<Cell> walkableCells = new List<Cell>();
//            IEnumerable<Point> nextLocations = GetAdjacentLocations(fromCell.Location);

//            foreach (var location in nextLocations)
//            {
//                int x = location.X;
//                int y = location.Y;

//                // Stay within the grid's boundaries
//                if (x < 0 || x >= this.width || y < 0 || y >= this.height)
//                    continue;

//                Cell Cell = this.Cells[x, y];
//                // Ignore non-walkable Cells
//                if (Cell.Tipo != Cell.TipoCelula.Edificio)
//                    continue;

//                // Ignore already-closed Cells
//                if (Cell.Estado == Cell.TipoEstado.Fechado)
//                    continue;

//                // Already-open Cells are only added to the list if their G-value is lower going via this route.
//                if (Cell.Estado == Cell.TipoEstado.Aberto)
//                {
//                    float traversalCost = Cell.GetTraversalCost(Cell.Location, Cell.ParentCell.Location);
//                    float gTemp = fromCell.G + traversalCost;
//                    if (gTemp < Cell.G)
//                    {
//                        Cell.ParentCell = fromCell;
//                        walkableCells.Add(Cell);
//                    }
//                }
//                else
//                {
//                    // If it's untested, set the parent and flag it as 'Open' for consideration
//                    Cell.ParentCell = fromCell;
//                    Cell.Estado = Cell.TipoEstado.Aberto;
//                    walkableCells.Add(Cell);
//                }
//            }

//            return walkableCells;
//        }

//        /// <summary>
//        /// Returns the eight locations immediately adjacent (orthogonally and diagonally) to <paramref name="fromLocation"/>
//        /// </summary>
//        /// <param name="fromLocation">The location from which to return all adjacent points</param>
//        /// <returns>The locations as an IEnumerable of Points</returns>
//        private static IEnumerable<Point> GetAdjacentLocations(Point fromLocation)
//        {
//            return new Point[]
//            {
//                new Point(fromLocation.X-1, fromLocation.Y-1),
//                new Point(fromLocation.X-1, fromLocation.Y  ),
//                new Point(fromLocation.X-1, fromLocation.Y+1),
//                new Point(fromLocation.X,   fromLocation.Y+1),
//                new Point(fromLocation.X+1, fromLocation.Y+1),
//                new Point(fromLocation.X+1, fromLocation.Y  ),
//                new Point(fromLocation.X+1, fromLocation.Y-1),
//                new Point(fromLocation.X,   fromLocation.Y-1)
//            };
//        }
//    }
//}
