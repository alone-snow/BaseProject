using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotWriteAttribute : PropertyAttribute { }

public class AutoWriteAttribute : Attribute { }

public class NotCloneAttribute : PropertyAttribute { }