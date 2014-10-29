namespace System
{
    /// <summary>
    /// Encapsulates a method that has one parameter and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T">the type of the parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg">the parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T, out TResult>(T arg);
    /// <summary>
    /// Encapsulates a method that has two parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg1">the first parameter of the method that this delegate encapsulates</param>
    /// <param name="arg2">the second parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T1, in T2, out TResult>(T1 arg1, T2 arg2);
    /// <summary>
    /// Encapsulates a method that has three parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <typeparam name="T3">the type of the third parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg1">the first parameter of the method that this delegate encapsulates</param>
    /// <param name="arg2">the second parameter of the method that this delegate encapsulates</param>
    /// <param name="arg3">the third parameter of the method that this delegate encapsulates</param>m>
    /// <returns></returns>
    public delegate TResult Func<in T1, in T2, in T3, out TResult>(T1 arg1, T2 arg2, T3 arg3);
    /// <summary>
    /// Encapsulates a method that has four parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <typeparam name="T3">the type of the third parameter</typeparam>
    /// <typeparam name="T4">the type of the fourth parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg1">the first parameter of the method that this delegate encapsulates</param>
    /// <param name="arg2">the second parameter of the method that this delegate encapsulates</param>
    /// <param name="arg3">the third parameter of the method that this delegate encapsulates</param>
    /// <param name="arg4">the fourth parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T1, in T2, in T3, in T4, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    /// <summary>
    /// Encapsulates a method that has five parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <typeparam name="T3">the type of the third parameter</typeparam>
    /// <typeparam name="T4">the type of the fourth parameter</typeparam>
    /// <typeparam name="T5">the type of the fifth parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg1">the first parameter of the method that this delegate encapsulates</param>
    /// <param name="arg2">the second parameter of the method that this delegate encapsulates</param>
    /// <param name="arg3">the third parameter of the method that this delegate encapsulates</param>
    /// <param name="arg4">the fourth parameter of the method that this delegate encapsulates</param>
    /// <param name="arg5">the fifth parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    /// <summary>
    /// Encapsulates a method that has six parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <typeparam name="T3">the type of the third parameter</typeparam>
    /// <typeparam name="T4">the type of the fourth parameter</typeparam>
    /// <typeparam name="T5">the type of the fifth parameter</typeparam>
    /// <typeparam name="T6">the type of the sixth parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg1">the first parameter of the method that this delegate encapsulates</param>
    /// <param name="arg2">the second parameter of the method that this delegate encapsulates</param>
    /// <param name="arg3">the third parameter of the method that this delegate encapsulates</param>
    /// <param name="arg4">the fourth parameter of the method that this delegate encapsulates</param>
    /// <param name="arg5">the fifth parameter of the method that this delegate encapsulates</param>
    /// <param name="arg6">the sixth parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    /// <summary>
    /// Encapsulates a method that has seven parameters and returns a value of the type specified by the <typeparamref name="TResult"/> parameter.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <typeparam name="T3">the type of the third parameter</typeparam>
    /// <typeparam name="T4">the type of the fourth parameter</typeparam>
    /// <typeparam name="T5">the type of the fifth parameter</typeparam>
    /// <typeparam name="T6">the type of the sixth parameter</typeparam>
    /// <typeparam name="T7">the type of the seventh parameter</typeparam>
    /// <typeparam name="TResult">the type of the return value</typeparam>
    /// <param name="arg1">the first parameter of the method that this delegate encapsulates</param>
    /// <param name="arg2">the second parameter of the method that this delegate encapsulates</param>
    /// <param name="arg3">the third parameter of the method that this delegate encapsulates</param>
    /// <param name="arg4">the fourth parameter of the method that this delegate encapsulates</param>
    /// <param name="arg5">the fifth parameter of the method that this delegate encapsulates</param>
    /// <param name="arg6">the sixth parameter of the method that this delegate encapsulates</param>
    /// <param name="arg7">the seventh parameter of the method that this delegate encapsulates</param>
    /// <returns></returns>
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    /// <summary>
    /// Encapsulates a method that has no parameter and does not return a value.
    /// </summary>
    public delegate void Action();
    /// <summary>
    /// Encapsulates a method that has two parameters and does not return a value.
    /// </summary>
    /// <typeparam name="T1">the type of the first parameter</typeparam>
    /// <typeparam name="T2">the type of the second parameter</typeparam>
    /// <param name="arg1">the first parameter</param>
    /// <param name="arg2">the second parameter</param>
    public delegate void Action<in T1, in T2>(T1 arg1, T2 arg2);
}
