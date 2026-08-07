# Tyle

Tyle is a simple eso-lang i made, It has 8/16/48 registers for fast storage and 768/1024/8192 for large storage.

When running code, your file must be named `code.tyle` unless it doesn't work (right now.)

You can use the `-core1` flag to restrict your resources down to 8 registers and 768 large store registers.

or use the `-core3` flag to bump it to 48 and 8192.

## Beware: Documentation isn't fully done so you might need to figure some stuff out from the source code.

### Comments
To make comments you use `{` to start a comment and `}` to end a comment.

### Printing and scanning numerals.
```tyle
{ First we set our string buffer to the desired output. }
str "Hello world!"

{ Then we call the print syscall }
syscall [print]

{ To read numerals use the scan syscall, And you can also optionally set a string prompt }
str "Enter number: "
syscall [scan]

{ To show a register's value, Use 'sel' to select it and then the rprint syscall to print it }
sel #1 { Registers and Large store addresses start at 1 }
syscall [rprint]
```

### Setting and Duping registers
```tyle
reg #1 49 { reg 1 is now 49 }

dup #1 #2 { Dup register 1 to 2 }
```


### Shifitng registers (Addition and subtraction)
```tyle
{ Changing a register by a literal }
sft #1 -40
sft #1 20

{ Changing a register by another register }
reg #2 49
sft #1 #2
```

### Halting
```tyle
sel #8
halt

{ the halt keyword halts the program with the exit code as the selected register. }
```

### Conditions
```tyle
reg #1 40
sel #1
if larger 30 { You can also use a register as the operand }
    str "40 is larger than 30"
    syscall [print]
end

{ Other check types include:
    equ
    smaller
    notequ
}
```

### Loops
```tyle
reg #1 20
until #1 0 { You can use a register here too }
{ The loop runs until register 1 is equal to the second value or register. }
sft #1 -1
end

{ You can use break to break out of a loop }
```

### Large store memory management
```tyle

{ Managing to hard-coded address }
reg #1 20
sel #1 { Select register 1 }
store $30 { $n is used for addresses, This writes the selected register's value to address 30 }

sel #1 { Unneccesary since we already selected 1 }
load $30 { Read address 30 into selected register. }

{ Managing indexed addresses }
reg #2 10 { This is our address }

sel #1
storei #2 { Uses register 2's value as the address and selected register as the value. }

sel #1
loadi #2 { Uses register 2's value as the address}

```

With indexed-writes and indexed-reads you can make arrays like how you would with pointers in C!