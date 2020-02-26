//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
#define DEBUGON

using System;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Collections;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Algoritmos
{
    public class PathFinderFast : IPathFinder
    {
        #region Structs
	    [StructLayout(LayoutKind.Sequential, Pack=1)] 
        internal struct PathFinderNodeFast
        {
            #region Variables Declaration
            public int     F; // f = gone + heuristic
            public int     G;
            public ushort  PX; // Parent
            public ushort  PY;
            public byte    Status;
            #endregion
        }
        #endregion

        #region Win32APIs
        //[System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint="RtlZeroMemory")]
        //public unsafe static extern bool ZeroMemory(byte* destination, int length);
        #endregion

        #region Events
        public event PathFinderDebugHandler PathFinderDebug;
        #endregion

        #region Variables Declaration
        // inicializa��o das vari�veis
        private byte[,]                         mGrid                   = null;
        private PriorityQueueB<int>             mOpen                   = null;
        private List<Node>            mClose                  = new List<Node>();
        private bool                            mStop                   = false;
        private bool                            mStopped                = true;
        private int                             mHoriz                  = 0;
        private HeuristicFormula                mFormula                = HeuristicFormula.Manhattan;
        private bool                            mDiagonals              = true;
        private int                             mHEstimate              = 2;
        private bool                            mPunishChangeDirection  = false;
        private bool                            mReopenCloseNodes       = true;
        private bool                            mTieBreaker             = false;
        private bool                            mHeavyDiagonals         = false;
        private int                             mSearchLimit            = 5000;
        private double                          mCompletedTime          = 0;
        private bool                            mDebugProgress          = false;
        private bool                            mDebugFoundPath         = false;
        private PathFinderNodeFast[]            mCalcGrid               = null;
        private byte                            mOpenNodeValue          = 1;
        private byte                            mCloseNodeValue         = 2;
        
        //vari�veis utilizadas para evitar re-cri��o entre as chamadas
        private int                             mH                      = 0;
        private int                             mLocation               = 0;
        private int                             mNewLocation            = 0;
        private ushort                          mLocationX              = 0;
        private ushort                          mLocationY              = 0;
        private ushort                          mNewLocationX           = 0;
        private ushort                          mNewLocationY           = 0;
        private int                             mCloseNodeCounter       = 0;
        private ushort                          mGridX                  = 0;
        private ushort                          mGridY                  = 0;
        private ushort                          mGridXMinus1            = 0;
        private ushort                          mGridYLog2              = 0;
        private bool                            mFound                  = false;
        private sbyte[,]                        mDirection              = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
        private int                             mEndLocation            = 0;
        private int                             mNewG                   = 0;
        #endregion

        #region Constructors
        public PathFinderFast(byte[,] grid)
        {
            if (grid == null)
                throw new Exception("Grid cannot be null");

            //inicializa as vari�veis referentes ao mapa
            mGrid           = grid;
            mGridX          = (ushort) (mGrid.GetUpperBound(0) + 1);
            mGridY          = (ushort) (mGrid.GetUpperBound(1) + 1);
            mGridXMinus1    = (ushort) (mGridX - 1);
            mGridYLog2      = (ushort) Math.Log(mGridY, 2);

            //uma limita��o do algoritmo � que o mapa deve ser pot�ncia de 2
            if (Math.Log(mGridX, 2) != (int) Math.Log(mGridX, 2) ||
                Math.Log(mGridY, 2) != (int) Math.Log(mGridY, 2))
                throw new Exception("Mapa inv�lido, o tamanho deve ser pot�ncia de 2");

            if (mCalcGrid == null || mCalcGrid.Length != (mGridX * mGridY))
                mCalcGrid = new PathFinderNodeFast[mGridX * mGridY];

            mOpen   = new PriorityQueueB<int>(new ComparePFNodeMatrix(mCalcGrid));
        }
        #endregion

        #region Properties
        public bool Stopped
        {
            get { return mStopped; }
        }

        public HeuristicFormula Formula
        {
            get { return mFormula; }
            set { mFormula = value; }
        }

        public bool Diagonals
        {
            get { return mDiagonals; }
            set 
            { 
                mDiagonals = value; 
                if (mDiagonals)
                    mDirection = new sbyte[8,2]{{0,-1} , {1,0}, {0,1}, {-1,0}, {1,-1}, {1,1}, {-1,1}, {-1,-1}};
                else
                    mDirection = new sbyte[4,2]{{0,-1} , {1,0}, {0,1}, {-1,0}};
            }
        }

        public bool HeavyDiagonals
        {
            get { return mHeavyDiagonals; }
            set { mHeavyDiagonals = value; }
        }

        public int HeuristicEstimate
        {
            get { return mHEstimate; }
            set { mHEstimate = value; }
        }

        public bool PunishChangeDirection
        {
            get { return mPunishChangeDirection; }
            set { mPunishChangeDirection = value; }
        }

        public bool ReopenCloseNodes
        {
            get { return mReopenCloseNodes; }
            set { mReopenCloseNodes = value; }
        }

        public bool TieBreaker
        {
            get { return mTieBreaker; }
            set { mTieBreaker = value; }
        }

        public int SearchLimit
        {
            get { return mSearchLimit; }
            set { mSearchLimit = value; }
        }

        public double CompletedTime
        {
            get { return mCompletedTime; }
            set { mCompletedTime = value; }
        }

        public bool DebugProgress
        {
            get { return mDebugProgress; }
            set { mDebugProgress = value; }
        }

        public bool DebugFoundPath
        {
            get { return mDebugFoundPath; }
            set { mDebugFoundPath = value; }
        }
        #endregion

        #region Methods
        public void FindPathStop()
        {
            mStop = true;
        }

        public List<Node> FindPath(Point start, Point end)
        {
            lock(this)//regi�o cr�tica de mem�ria
            {
                HighResolutionTime.Start();

                mFound              = false;
                mStop               = false;
                mStopped            = false;
                mCloseNodeCounter   = 0;
                mOpenNodeValue      += 2;
                mCloseNodeValue     += 2;
                mOpen.Clear();
                mClose.Clear();


                //se a depura��o estiver ativa, retorna os passos do algoritmo, atrav�s de um evento.
                #if DEBUGON
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, start.X, start.Y, PathFinderNodeType.Start, -1, -1);
                if (mDebugProgress && PathFinderDebug != null)
                    PathFinderDebug(0, 0, end.X, end.Y, PathFinderNodeType.End, -1, -1);
                #endif

                mLocation                      = (start.Y << mGridYLog2) + start.X;
                mEndLocation                   = (end.Y << mGridYLog2) + end.X;
                mCalcGrid[mLocation].G         = 0;
                mCalcGrid[mLocation].F         = mHEstimate;
                mCalcGrid[mLocation].PX        = (ushort) start.X;
                mCalcGrid[mLocation].PY        = (ushort) start.Y;
                mCalcGrid[mLocation].Status    = mOpenNodeValue;

                mOpen.Push(mLocation);
                while(mOpen.Count > 0 && !mStop)
                {
                    mLocation    = mOpen.Pop();

                    //Se o n� estiver fechado, ele j� foi processado, ent�o continue
                    if (mCalcGrid[mLocation].Status == mCloseNodeValue)
                        continue;

                    mLocationX   = (ushort) (mLocation & mGridXMinus1);
                    mLocationY   = (ushort) (mLocation >> mGridYLog2);
                    
                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(0, 0, mLocation & mGridXMinus1, mLocation >> mGridYLog2, PathFinderNodeType.Current, -1, -1);
                    #endif

                    //encontrou uma solu��o
                    if (mLocation == mEndLocation)
                    {
                        mCalcGrid[mLocation].Status = mCloseNodeValue;
                        mFound = true;
                        break;
                    }

                    //se configurado um limite
                    if (mCloseNodeCounter > mSearchLimit)
                    {
                        mStopped = true;
                        mCompletedTime = HighResolutionTime.GetTime();
                        return null;
                    }
                    //se puni��o estiver configurada
                    if (mPunishChangeDirection)
                        mHoriz = (mLocationX - mCalcGrid[mLocation].PX); 

                    //calcula os successores do n� atual (considerando diagonais ou n�o)
                    for (int i=0; i<(mDiagonals ? 8 : 4); i++)
                    {
                        mNewLocationX = (ushort) (mLocationX + mDirection[i,0]);
                        mNewLocationY = (ushort) (mLocationY + mDirection[i,1]);
                        mNewLocation  = (mNewLocationY << mGridYLog2) + mNewLocationX;

                        //se a coordenada atual excede os limites do mapa, avan�a
                        if (mNewLocationX >= mGridX || mNewLocationY >= mGridY)
                            continue;

                        //se reabertura de nos estiver desligado e o n� fechado, avan�a
                        if (mCalcGrid[mNewLocation].Status == mCloseNodeValue && !mReopenCloseNodes)
                            continue;

                        // se o n� n�o � caminh�vel, avan�a
                        if (mGrid[mNewLocationX, mNewLocationY] == 0)
                            continue;

                        if (mHeavyDiagonals && i>3)
                            mNewG = mCalcGrid[mLocation].G + (int) (mGrid[mNewLocationX, mNewLocationY] * 2.41);
                        else
                            mNewG = mCalcGrid[mLocation].G + mGrid[mNewLocationX, mNewLocationY];

                        //se puni��o est� ativa, calcule
                        if (mPunishChangeDirection)
                        {
                            if ((mNewLocationX - mLocationX) != 0)
                            {
                                if (mHoriz == 0)
                                    mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                            }
                            if ((mNewLocationY - mLocationY) != 0)
                            {
                                if (mHoriz != 0)
                                    mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
                            }
                        }

                        //verifica se o n� est� aberto ou fechado
                        if (mCalcGrid[mNewLocation].Status == mOpenNodeValue || mCalcGrid[mNewLocation].Status == mCloseNodeValue)
                        {
                            // O n� atual tem menos c�digo do que o anterior? ent�o pule este n�
                            if (mCalcGrid[mNewLocation].G <= mNewG)
                                continue;
                        }

                        mCalcGrid[mNewLocation].PX      = mLocationX;
                        mCalcGrid[mNewLocation].PY      = mLocationY;
                        mCalcGrid[mNewLocation].G       = mNewG;

                        switch(mFormula)
                        {
                            default:
                            case HeuristicFormula.Manhattan:
                                mH = mHEstimate * (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                                break;
                            case HeuristicFormula.MaxDXDY:
                                mH = mHEstimate * (Math.Max(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y)));
                                break;
                            case HeuristicFormula.DiagonalShortCut:
                                int h_diagonal  = Math.Min(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
                                int h_straight  = (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
                                mH = (mHEstimate * 2) * h_diagonal + mHEstimate * (h_straight - 2 * h_diagonal);
                                break;
                            case HeuristicFormula.Euclidean:
                                mH = (int) (mHEstimate * Math.Sqrt(Math.Pow((mNewLocationY - end.X) , 2) + Math.Pow((mNewLocationY - end.Y), 2)));
                                break;
                            case HeuristicFormula.EuclideanNoSQR:
                                mH = (int) (mHEstimate * (Math.Pow((mNewLocationX - end.X) , 2) + Math.Pow((mNewLocationY - end.Y), 2)));
                                break;
                            case HeuristicFormula.Custom1:
                                Point dxy       = new Point(Math.Abs(end.X - mNewLocationX), Math.Abs(end.Y - mNewLocationY));
                                int Orthogonal  = Math.Abs(dxy.X - dxy.Y);
                                int Diagonal    = Math.Abs(((dxy.X + dxy.Y) - Orthogonal) / 2);
                                mH = mHEstimate * (Diagonal + Orthogonal + dxy.X + dxy.Y);
                                break;
                        }
                        //em caso de multiplos caminhos a seguir, usar fun��o de desempate
                        if (mTieBreaker)
                        {
                            int dx1 = mLocationX - end.X;
                            int dy1 = mLocationY - end.Y;
                            int dx2 = start.X - end.X;
                            int dy2 = start.Y - end.Y;
                            int cross = Math.Abs(dx1 * dy2 - dx2 * dy1);
                            mH = (int) (mH + cross * 0.001);
                        }
                        mCalcGrid[mNewLocation].F = mNewG + mH;

                        //se depura��o estiver ativa, informa os passos do algoritmo
                        #if DEBUGON
                        if (mDebugProgress && PathFinderDebug != null)
                            PathFinderDebug(mLocationX, mLocationY, mNewLocationX, mNewLocationY, PathFinderNodeType.Open, mCalcGrid[mNewLocation].F, mCalcGrid[mNewLocation].G);
                        #endif


                        mOpen.Push(mNewLocation);

                        mCalcGrid[mNewLocation].Status = mOpenNodeValue;
                    }

                    mCloseNodeCounter++;
                    mCalcGrid[mLocation].Status = mCloseNodeValue;

                    #if DEBUGON
                    if (mDebugProgress && PathFinderDebug != null)
                        PathFinderDebug(0, 0, mLocationX, mLocationY, PathFinderNodeType.Close, mCalcGrid[mLocation].F, mCalcGrid[mLocation].G);
                    #endif
                }
                //tempo total gasto para a execu��o
                mCompletedTime = HighResolutionTime.GetTime();

                //se encontrou uma solu��o, adiciona na lista de solu��o mClose
                if (mFound)
                {
                    mClose.Clear();
                    int posX = end.X;
                    int posY = end.Y;

                    PathFinderNodeFast fNodeTmp = mCalcGrid[(end.Y << mGridYLog2) + end.X];
                    Node fNode;
                    fNode.F  = fNodeTmp.F;
                    fNode.G  = fNodeTmp.G;
                    fNode.H  = 0;
                    fNode.PX = fNodeTmp.PX;
                    fNode.PY = fNodeTmp.PY;
                    fNode.X  = end.X;
                    fNode.Y  = end.Y;

                    while(fNode.X != fNode.PX || fNode.Y != fNode.PY)
                    {
                        mClose.Add(fNode);
                        #if DEBUGON
                        if (mDebugFoundPath && PathFinderDebug != null)
                            PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
                        #endif
                        posX = fNode.PX;
                        posY = fNode.PY;
                        fNodeTmp = mCalcGrid[(posY << mGridYLog2) + posX];
                        fNode.F  = fNodeTmp.F;
                        fNode.G  = fNodeTmp.G;
                        fNode.H  = 0;
                        fNode.PX = fNodeTmp.PX;
                        fNode.PY = fNodeTmp.PY;
                        fNode.X  = posX;
                        fNode.Y  = posY;
                    } 

                    mClose.Add(fNode);
                    #if DEBUGON
                    if (mDebugFoundPath && PathFinderDebug != null)
                        PathFinderDebug(fNode.PX, fNode.PY, fNode.X, fNode.Y, PathFinderNodeType.Path, fNode.F, fNode.G);
                    #endif

                    mStopped = true;
                    return mClose;
                }
                mStopped = true;
                return null;
            }
        }
        #endregion

        #region Inner Classes
        internal class ComparePFNodeMatrix : IComparer<int>
        {
            #region Variables Declaration
            PathFinderNodeFast[] mMatrix;
            #endregion

            #region Constructors
            public ComparePFNodeMatrix(PathFinderNodeFast[] matrix)
            {
                mMatrix = matrix;
            }
            #endregion

            #region IComparer Members
            public int Compare(int a, int b)
            {
                if (mMatrix[a].F > mMatrix[b].F)
                    return 1;
                else if (mMatrix[a].F < mMatrix[b].F)
                    return -1;
                return 0;
            }
            #endregion
        }
        #endregion
    }
    #region Delegates
    public delegate void PathFinderDebugHandler(int fromX, int fromY, int x, int y, PathFinderNodeType type, int totalCost, int cost);
    #endregion

    public enum HeuristicFormula
    {
        Manhattan = 1,
        MaxDXDY = 2,
        DiagonalShortCut = 3,
        Euclidean = 4,
        EuclideanNoSQR = 5,
        Custom1 = 6
    }
    public enum PathFinderNodeType
    {
        Start = 1,
        End = 2,
        Open = 4,
        Close = 8,
        Current = 16,
        Path = 32
    }
}
