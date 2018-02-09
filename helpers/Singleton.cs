public class Singleton<T> where T : class, new() {
    private static readonly T _instance = new T();
    public static T Instance => _instance;
    static Singleton() {} // Prevent beforefieldinit flag; Avoid instance creation before first member call
    protected Singleton() {}
}