// PictApiWrapper.cpp
#include <stddef.h> 
#include "../api/pictapi.h"
#include <stdlib.h>

// Make sure we own these macros and avoid redefinition warnings
#ifdef PICT_WRAPPER_API
#undef PICT_WRAPPER_API
#endif

#ifdef PICT_WRAPPER_CALL
#undef PICT_WRAPPER_CALL
#endif

#if defined(_WIN32)
#define PICT_WRAPPER_API  __declspec(dllexport)
#define PICT_WRAPPER_CALL __cdecl
#else
#define PICT_WRAPPER_API
#define PICT_WRAPPER_CALL
#endif

extern "C"
{
    PICT_WRAPPER_API
        PICT_HANDLE PICT_WRAPPER_CALL PictCreateTask_Wrapper()
    {
        return PictCreateTask();
    }

    PICT_WRAPPER_API
        PICT_HANDLE PICT_WRAPPER_CALL PictCreateModel_Wrapper(unsigned int randomSeed)
    {
        return PictCreateModel(randomSeed);
    }

    // High-level helper that C# calls directly
    PICT_WRAPPER_API
        int PICT_WRAPPER_CALL PictGenerateIndices(
            const size_t* valueCounts,
            size_t        paramCount,
            unsigned int  order,
            unsigned int  randomSeed,
            int** outCells,
            size_t* outRowCount)
    {
        auto task = PictCreateTask();
        if (!task) return PICT_OUT_OF_MEMORY;

        auto model = PictCreateModel(randomSeed);
        if (!model)
        {
            PictDeleteTask(task);
            return PICT_OUT_OF_MEMORY;
        }

        PictSetRootModel(task, model);

        for (size_t i = 0; i < paramCount; ++i)
            PictAddParameter(model, valueCounts[i], order, nullptr);

        auto rc = PictGenerate(task);
        if (rc != PICT_SUCCESS)
        {
            PictDeleteTask(task);
            PictDeleteModel(model);
            *outCells = nullptr;
            *outRowCount = 0;
            return rc;
        }

        PictResetResultFetching(task);
        auto cols = PictGetTotalParameterCount(task);
        auto row = PictAllocateResultBuffer(task);

        size_t rows = 0;
        for (;;)
        {
            if (!PictGetNextResultRow(task, row))
                break;
            ++rows;
        }

        PictResetResultFetching(task);

        auto cells = static_cast<int*>(malloc(rows * cols * sizeof(int)));
        if (!cells)
        {
            PictFreeResultBuffer(row);
            PictDeleteTask(task);
            PictDeleteModel(model);
            *outCells = nullptr;
            *outRowCount = 0;
            return PICT_OUT_OF_MEMORY;
        }

        auto p = cells;
        for (size_t r = 0; r < rows; ++r)
        {
            PictGetNextResultRow(task, row);
            for (size_t c = 0; c < cols; ++c)
                *p++ = row[c];
        }

        PictFreeResultBuffer(row);
        PictDeleteTask(task);
        PictDeleteModel(model);

        *outCells = cells;
        *outRowCount = rows;
        return PICT_SUCCESS;
    }

    PICT_WRAPPER_API
        void PICT_WRAPPER_CALL PictFreeIndices(int* cells)
    {
        free(cells);
    }
}
