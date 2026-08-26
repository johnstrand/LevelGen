namespace LevelGen.Tests;

/// <summary>
/// Contains reusable prefab definitions and block parser test strings used across test suites.
/// </summary>
public static class TestPrefabs
{
    /// <summary>
    /// A 3x3 room with top and bottom wall rows, and side connectors.
    /// </summary>
    public const string Standard3x3Room = """
        > Room
        ###
        *.*
        ###
        """;

    /// <summary>
    /// A 1x1 room with a single floor tile.
    /// </summary>
    public const string SmallRoom1x1 = """
        > SmallRoom
        .
        """;

    /// <summary>
    /// A simple enclosed 3x3 room (Room1) with leading comment.
    /// </summary>
    public const string SinglePrefabWithComment = """
        // This is a comment
        > Room1
        ###
        #.#
        ###
        """;

    /// <summary>
    /// A simple enclosed 3x3 room (Room1).
    /// </summary>
    public const string Enclosed3x3Room = """
        > Room1
        ###
        #.#
        ###
        """;

    /// <summary>
    /// A room showcasing all tile kinds and doodad markers.
    /// </summary>
    public const string AllTokensAndDoodadsRoom = """
        > AllTokens
        #.* ?A P
        """;

    /// <summary>
    /// A room with uneven row lengths that tests line padding.
    /// </summary>
    public const string UnevenRowsRoom = """
        > Uneven
        ###
        #
        #####
        """;

    /// <summary>
    /// A multi-prefab definition containing two separate prefabs with comments and blank lines.
    /// </summary>
    public const string MultiplePrefabsWithComments = """
        // First prefab
        > First
        ..

        // Second prefab separated by empty line
        > Second
        ##
        ##
        """;

    /// <summary>
    /// A room definition containing carriage return line endings.
    /// </summary>
    public const string CarriageReturnRoom = "> CRTest\r\n#.\r\n.#\r\n";

    /// <summary>
    /// Malformed block input: tiles appearing before a section header.
    /// </summary>
    public const string TilesBeforeHeader = """
        ###
        > Header
        ###
        """;

    /// <summary>
    /// Malformed block input: header with a blank prefab name.
    /// </summary>
    public const string BlankHeaderName = "> \n .#";

    /// <summary>
    /// Malformed block input: header with only whitespace name space.
    /// </summary>
    public const string HeaderOnlySpace = "> ";

    /// <summary>
    /// Malformed block input: header with only tab character as name.
    /// </summary>
    public const string HeaderOnlyTab = ">\t";

    /// <summary>
    /// Malformed block input: header with only carriage return newline.
    /// </summary>
    public const string HeaderOnlyCRLF = ">\r\n";

    /// <summary>
    /// Malformed block input: contains an unsupported tile character (+).
    /// </summary>
    public const string UnsupportedTileToken = """
        > Invalid
        #+#
        """;

    /// <summary>
    /// Malformed block input: contains header but no tile rows.
    /// </summary>
    public const string HeaderOnlyWithNoRows = "> HeaderOnlyWithNoRows\n\n";

    /// <summary>
    /// Malformed block input: tiles before section header (short format).
    /// </summary>
    public const string TilesBeforeHeaderShort = "..##\n> Name";

    /// <summary>
    /// Malformed block input: tiles before header with TestRoom.
    /// </summary>
    public const string TilesBeforeHeaderTestRoom = """
        #.
        > TestRoom
        #.
        """;

    /// <summary>
    /// Malformed block input: blank header name followed by tiles.
    /// </summary>
    public const string BlankHeaderFollowedByTiles = ">\n..##";

    /// <summary>
    /// Malformed block input: blank header name in multiline text block.
    /// </summary>
    public const string BlankHeaderInTextBlock = """
        >
        #.
        """;

    /// <summary>
    /// Malformed block input: room with unsupported '+' token.
    /// </summary>
    public const string RoomWithUnsupportedToken = "> Room\n..+";

    /// <summary>
    /// Malformed block input: test room with unsupported '+' token.
    /// </summary>
    public const string TestRoomWithUnsupportedToken = """
        > TestRoom
        #.
        #+
        """;

    /// <summary>
    /// Comment line only input for no-prefabs-found tests.
    /// </summary>
    public const string CommentOnlyInput = "// Just a comment line";

    /// <summary>
    /// Single slash comment line input for no-prefabs-found tests.
    /// </summary>
    public const string SingleSlashCommentInput = """
        / Just a comment line
        """;
}
