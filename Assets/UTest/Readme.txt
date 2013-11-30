======= Getting Stated =========

To Start testing all you have to do is make a class that implements UTest.IModule
and Decorate it with Utest.Module Attribute ([UTest.Module]). A test is just a method inside that class
decorated with Utest.Test Attribute  ([UTest.Test]).

======= Tests & Modules =========

Every test must be contained inside a module(a class).Test and module attibutes both support custom namming 
and description, for easy spotting on the UTest window.

The constructor of a module class will be called once prior running it's tests, and it's dispose method will be
called when UTest is done with your module. Per module setup and cleanup should be done on the constructor and 
dispose methods of a module, respectively.

The WarmUp and TearDown methods of a module are called before and after each test, respectively. Per test setup 
and cleanup should be done inside this methods.

A module can be any class with a trivial constructor, including nested classes, and can be even inside Editor 
folders.

====== Asserts ======

UTest has a very minimalistic aproach to assertions providing you with:

UTest.Assert.Truth -> Test boolean values
UTest.Assert.Equality -> Test equality between 2 objects(using equals)
UTest.Assert.Throws -> Test if a piece of code throws a given exception

This three assertions should be all you need to test anything!


===== UTest window =======

Window->UTest->Open View : Will open you UTest window

On that window you'll see all your test organized in a fouldout hierarchy, with root as it's, well, root
and each indiviaual test as a leaf. Between then will be your modules.

On the right side of each node(root, module or test), you'll find a "Run" button. If Pressed, UTest will run
All thest inside that node, be it a single test, all the test in a module or all your tests, if you press root's
"Run" button.

On the right side of the Run "button" you'll find a check box. When checked you will add that node, and all it's
children, to the Selection. Unheck to remove.

By hovering the mouse ouver a node, a pop-up will apear, showing you that node's description.

In the bottom of the window you'll find 2 buttons:

Run Selection: Will run all selected nodes
Clear Selection: Will unselect all nodes

The result of any run will be displayed at the bottom of the window, right above the buttons. In case of full 
success a simple message will appear telling you how many test have passed.
In case of failure, A big red message will apear, telling you information about the test that just failed and a 
also new button will appear, "Open offender", that when pressed will open the script editor on the line and file 
of the cause of the failure.

The window run the nodes 'till completion or 'till it find a failling test. If you want to run all tests 
regardless Of failures, press ctrl+shift+T.


===== Keyboard shortcuts ======

ctrl+T : Open UTest Window
ctrl+shift+T : Run all tests and print the results on the console

====== Contact Me (threers33@gmail.com) =======

Got other questions, a bug report our any other problem?
Please, enter in contact!

Email me at threers33@gmail.com