using MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Models;

namespace MICHAŁ_PAMIĘTA_196099_PRACA_MAGISTERSKA.Utils
{
    public static class NeighborhoodHelper
    {
        public static List<Individual> GetNeighborhood1D(Individual[] population, int index, int range, bool wrap)
        {
            List<Individual> neighbors = [];

            for (int offset = -range; offset <= range; offset++)
            {
                int neighborIndex = index + offset;

                if (wrap)
                {
                    neighborIndex = (neighborIndex + population.Length) % population.Length;
                }
                else if (neighborIndex < 0 || neighborIndex >= population.Length)
                {
                    continue;
                }

                if (population[neighborIndex] != null)
                {
                    neighbors.Add(population[neighborIndex]);
                }
            }

            return neighbors;
        }

        public static List<Individual> GetNeighbors2D(int i, int j, Individual[,] population, NeighborhoodType type, int range = 1, bool wrap = true)
        {
            int rows = population.GetLength(0);
            int cols = population.GetLength(1);

            List<Individual> neighbors = [];

            for (int di = -range; di <= range; di++)
            {
                for (int dj = -range; dj <= range; dj++)
                {
                    if (type == NeighborhoodType.VonNeumann && Math.Abs(di) + Math.Abs(dj) > range)
                    {
                        continue;
                    }

                    int ni = i + di;
                    int nj = j + dj;

                    if (wrap)
                    {
                        ni = (ni + rows) % rows;
                        nj = (nj + cols) % cols;
                    }
                    else
                    {
                        if (ni < 0 || ni >= rows || nj < 0 || nj >= cols)
                        {
                            continue;
                        }
                    }

                    if (population[ni, nj] != null)
                    {
                        neighbors.Add(population[ni, nj]);
                    }
                }
            }

            return neighbors;
        }

        public static List<Individual> GetNeighbors3D(int x, int y, int z, Individual[,,] population, NeighborhoodType type, int range = 1, bool wrap = true)
        {
            int dimX = population.GetLength(0);
            int dimY = population.GetLength(1);
            int dimZ = population.GetLength(2);

            List<Individual> neighbors = [];

            for (int dx = -range; dx <= range; dx++)
            {
                for (int dy = -range; dy <= range; dy++)
                {
                    for (int dz = -range; dz <= range; dz++)
                    {
                        if (type == NeighborhoodType.VonNeumann && Math.Abs(dx) + Math.Abs(dy) + Math.Abs(dz) > range)
                        {
                            continue;
                        }

                        int nx = x + dx;
                        int ny = y + dy;
                        int nz = z + dz;

                        if (wrap)
                        {
                            nx = (nx + dimX) % dimX;
                            ny = (ny + dimY) % dimY;
                            nz = (nz + dimZ) % dimZ;
                        }
                        else
                        {
                            if (nx < 0 || nx >= dimX || ny < 0 || ny >= dimY || nz < 0 || nz >= dimZ)
                            {
                                continue;
                            }
                        }

                        if (population[nx, ny, nz] != null)
                        {
                            neighbors.Add(population[nx, ny, nz]);
                        }
                    }
                }
            }

            return neighbors;
        }

        public static (int rows, int cols) FindBest2DGridShape(int count)
        {
            int bestArea = int.MaxValue;
            int bestRows = 1, bestCols = count;

            for (int rows = 1; rows <= Math.Sqrt(count) + 1; rows++)
            {
                int cols = (int)Math.Ceiling((double)count / rows);
                int area = rows * cols;

                if (area < bestArea || (area == bestArea && Math.Abs(rows - cols) < Math.Abs(bestRows - bestCols)))
                {
                    bestArea = area;
                    bestRows = rows;
                    bestCols = cols;
                }
            }

            return (bestRows, bestCols);
        }

        public static (int rows, int cols, int depth) FindBest3DGridShape(int count)
        {
            int bestVolume = int.MaxValue;
            int bestRows = 1, bestCols = 1, bestDepth = count;

            int max = (int)Math.Ceiling(Math.Pow(count, 1.0 / 3)) + 1;

            for (int rows = 1; rows <= max; rows++)
            {
                for (int cols = rows; cols <= max; cols++)
                {
                    int depth = (int)Math.Ceiling((double)count / (rows * cols));
                    if (depth < cols)
                    {
                        continue;
                    }

                    int volume = rows * cols * depth;
                    if (volume < bestVolume)
                    {
                        bestVolume = volume;
                        bestRows = rows;
                        bestCols = cols;
                        bestDepth = depth;
                    }
                }
            }

            return (bestRows, bestCols, bestDepth);
        }
    }
}
