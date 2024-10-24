using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace OpenGLSilk;

internal class Program
{
    private static IWindow? _window;
    private static GL? _gl;
    private static uint _vao, _vbo, _shader;

    // Window dimensions
    private const int Width = 800;

    private const int Height = 600;

    // Shader sources
    private const string VertexShaderSource = """

                                                      #version 330 core
                                                      layout (location = 0) in vec3 position;
                                                      void main()
                                                      {
                                                          gl_Position = vec4(0.4 * position.x, 0.4 * position.y, position.z, 1.0);
                                                      }

                                              """;

    private const string FragmentShaderSource = """

                                                        #version 330 core
                                                        out vec4 color;
                                                        void main()
                                                        {
                                                            color = vec4(0.8f, 0.5f, 0.1f, 1.0f);
                                                        }

                                                """;

    private static void Main()
    {
        WindowOptions options = WindowOptions.Default;
        options.Size = new Vector2D<int>(Width, Height);
        options.Title = "Test Window";

        _window = Window.Create(options);

        _window.Load += OnLoad;
        _window.Render += OnRender;
        _window.Closing += OnClose;

        _window.Run();
    }

    private static void OnLoad()
    {
        // Get OpenGL API
        _gl = GL.GetApi(_window);

        CreateTriangle();
        CompileShader();
    }

    private static unsafe void CreateTriangle()
    {
        float[] vertices =
        [
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            0.0f, 1.0f, 0.0f
        ];

        // Generate and bind VAO
        if (_gl != null)
        {
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            // Generate, bind and fill VBO
            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (ReadOnlySpan<float>)vertices, BufferUsageARB.StaticDraw);

            // Set vertex attributes
            _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);
            _gl.EnableVertexAttribArray(0);

            // Unbind
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            _gl.BindVertexArray(0);
        }
    }

    private static void CompileShader()
    {
        // Create shader program
        if (_gl != null)
        {
            _shader = _gl.CreateProgram();

            // Create and compile vertex shader
            uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShader, VertexShaderSource);
            _gl.CompileShader(vertexShader);
            CheckShaderError(vertexShader, ShaderType.VertexShader);

            // Create and compile fragment shader
            uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShader, FragmentShaderSource);
            _gl.CompileShader(fragmentShader);
            CheckShaderError(fragmentShader, ShaderType.FragmentShader);

            // Attach shaders to program
            _gl.AttachShader(_shader, vertexShader);
            _gl.AttachShader(_shader, fragmentShader);

            // Link program
            _gl.LinkProgram(_shader);

            // Check for errors
            CheckProgramError(_shader);

            // Delete shaders as they're linked into our program and no longer necessary
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);
        }
    }

    private static void CheckShaderError(uint shader, ShaderType type)
    {
        string? infoLog = _gl?.GetShaderInfoLog(shader);

        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error compiling {type} shader: {infoLog}");
        }
    }

    private static void CheckProgramError(uint program)
    {
        string? infoLog = _gl?.GetProgramInfoLog(program);

        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            throw new Exception($"Error linking program: {infoLog}");
        }
    }

    private static void OnRender(double deltaTime)
    {
        // Clear the color buffer
        _gl?.ClearColor(0.25f, 0.25f, 0.25f, 1.0f);
        _gl?.Clear(ClearBufferMask.ColorBufferBit);

        // Draw triangle
        _gl?.UseProgram(_shader);
        _gl?.BindVertexArray(_vao);
        _gl?.DrawArrays(PrimitiveType.Triangles, 0, 3);

        // Unbind
        _gl?.BindVertexArray(0);

        // Unbind program
        _gl?.UseProgram(0);
    }

    private static void OnClose()
    {
        // Clean up resources
        _gl?.DeleteBuffer(_vbo);
        _gl?.DeleteVertexArray(_vao);
        _gl?.DeleteProgram(_shader);
    }
}