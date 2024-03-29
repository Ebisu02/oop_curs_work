﻿using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame
{
    class Game
    {
        static readonly int _x = 80;
        static readonly int _y = 26;

        static Walls walls;
        static Snake snake;
        static FoodFactory food_factory;
        static Timer time;

        static void Main()
        {
            Console.SetWindowSize(_x + 1, _y + 1);
            Console.SetBufferSize(_x + 1, _y + 1);
            Console.CursorVisible = false;

            walls = Walls
                { x = _x, y = _y, ch = '#' };
            snake = new Snake(x / 2, y / 2, 3);

            food_factory = new FoodFactory(x, y, '@');
            food_factory.CreateFood();

            time = new Timer(Loop, null, 0, 200);

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey();
                    snake.Rotation(key.Key);
                }
            }
        }// Main()

        static void Loop(object obj)
        {
            if (walls.IsHit(snake.GetHead()) || snake.IsHit(snake.GetHead()))
            {
                time.Change(0, Timeout.Infinite);
            }
            else if (snake.Eat(food_factory.food))
            {
                food_factory.CreateFood();
            }
            else
            {
                snake.Move();
            }
        }// Loop()
    }// class Game

    struct Point
    {
        public int x { get; set; }
        public int y { get; set; }
        public char ch { get; set; }

        public static implicit operator Point((int, int, char) value) =>
              new Point { x = value.Item1, y = value.Item2, ch = value.Item3 };

        public static bool operator ==(Point a, Point b) =>
            /*
             * if (a.x == b.x && a.y == b.y)
             *      return true;
             * else 
             *      return false;
             */
                (a.x == b.x && a.y == b.y) ? true : false;
        public static bool operator !=(Point a, Point b) =>
                (a.x != b.x || a.y != b.y) ? true : false;

        public void Draw()
        {
            DrawPoint(ch);
        }
        public void Clear()
        {
            DrawPoint(' ');
        }

        private void DrawPoint(char _ch)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(_ch);
        }
    }

    interface IWalls
    {
        virtual public bool IsHit(Point p) { return true; }
    };


    class Walls: IWalls
    {
        private char ch;
        private List<Point> wall = new List<Point>();

        public Walls(int x, int y, char ch)
        {
            this.ch = ch;

            DrawHorizontal(x, 0);
            DrawHorizontal(x, y);
            DrawVertical(0, y);
            DrawVertical(x, y);
        }

        private void DrawHorizontal(int x, int y)
        {
            for (int i = 0; i < x; i++)
            {
                Point p = (i, y, ch);
                p.Draw();
                wall.Add(p);
            }
        }

        private void DrawVertical(int x, int y)
        {
            for (int i = 0; i < y; i++)
            {
                Point p = (x, i, ch);
                p.Draw();
                wall.Add(p);
            }
        }

        virtual public bool IsHit(Point p)
        {
            foreach (var w in wall)
            {
                if (p == w)
                {
                    return true;
                }
            }
            return false;
        }
    }// class Walls

    enum Direction
    {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    interface ISnake
    {
        public Point GetHead();
        public void Move();
        public bool Eat(Point p);
        public Point GetNextPoint();
        public void Rotation(ConsoleKey key);
        public bool IsHit(Point p);
    }


    class Snake: ISnake
    {
        private List<Point> snake;

        private Direction direction;
        private int step = 1;
        private Point tail;
        private Point head;
        private Direction direction_control;
        bool rotate = true;

        public Snake(int x, int y, int length)
        {
            direction = Direction.RIGHT;

            snake = new List<Point>();
            for (int i = x - length; i < x; i++)
            {
                Point p = (i, y, '*');
                snake.Add(p);

                p.Draw();
            }
        }

        public Point GetHead() => snake.Last();

        public void Move()
        {
            head = GetNextPoint();
            snake.Add(head);

            tail = snake.First();
            snake.Remove(tail);

            tail.Clear();
            head.Draw();

            rotate = true;
        }

        public bool Eat(Point p)
        {
            head = GetNextPoint();
            if (head == p)
            {
                snake.Add(head);
                head.Draw();
                return true;
            }
            return false;
        }

        public Point GetNextPoint()
        {
            Point p = GetHead();

            switch (direction)
            {
                case Direction.LEFT:
                    p.x -= step;
                    break;
                case Direction.RIGHT:
                    p.x += step;
                    break;
                case Direction.UP:
                    p.y -= step;
                    break;
                case Direction.DOWN:
                    p.y += step;
                    break;
            }
            return p;
        }

        public void Rotation(ConsoleKey key)
        {
            if (rotate)
            {
                switch (direction_control)
                {
                    case Direction.LEFT:
                    case Direction.RIGHT:
                        if (key == ConsoleKey.DownArrow)
                            direction_control = Direction.DOWN;
                        else if (key == ConsoleKey.UpArrow)
                            direction_control = Direction.UP;
                        break;
                    case Direction.UP:
                    case Direction.DOWN:
                        if (key == ConsoleKey.LeftArrow)
                            direction_control = Direction.LEFT;
                        else if (key == ConsoleKey.RightArrow)
                            direction_control = Direction.RIGHT;
                        break;
                }
                if ((direction == Direction.RIGHT && direction_control == Direction.LEFT) ||
                    (direction == Direction.LEFT && direction_control == Direction.RIGHT) ||
                    (direction == Direction.UP && direction_control == Direction.DOWN) ||
                    (direction == Direction.DOWN && direction_control == Direction.UP))
                {

                }
                else
                {
                    direction = direction_control;
                }
                rotate = false;
            }

        }

        public bool IsHit(Point p)
        {
            for (int i = snake.Count - 2; i > 0; i--)
            {
                try
                {
                    if (snake[i] == p)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
            return false;
        }
    }

    interface IFoodFactory
    {
        public void CreateFood();
    };


    class FoodFactory : IFoodFactory
    {
        int x;
        int y;
        char ch;
        public Point food { get; private set; }

        Random random = new Random();

        public FoodFactory()//: this.x, this.y(0), this.ch('\0')
        {
            x = 0;
            y = 0;
            ch = '\0';
        }

        public FoodFactory(int x, int y, char ch)
        {
            this.x = x;
            this.y = y;
            this.ch = ch;
        }

        public void CreateFood()
        {
            food = (random.Next(2, x - 2), random.Next(2, y - 2), ch);
            food.Draw();
        }
    }
}