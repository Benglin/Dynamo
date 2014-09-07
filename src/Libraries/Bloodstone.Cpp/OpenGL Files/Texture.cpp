
#include "stdafx.h"
#include "OpenInterfaces.h"
#include "BillboardText.h" // TODO: Move "BitmapData" to "OpenInterfaces.h"

using namespace Dynamo::Bloodstone;
using namespace Dynamo::Bloodstone::OpenGL;

Texture2d::Texture2d(const IGraphicsContext* pGraphicsContext) : 
    mTextureId(0),
    mTexAttribLoc(-1)
{
}

Texture2d::~Texture2d(void)
{
    if (mTextureId != 0) {
        // GL::glDeleteTextures(1, &mTextureId);
        mTextureId = 0;
        mTexAttribLoc = -1;
    }
}

void Texture2d::SetBitmapData(const BitmapData* pBitmapData)
{
    if (mTextureId == 0)
        GL::glGenTextures(1, &mTextureId);

    GL::glBindTexture(GL_TEXTURE_2D, mTextureId);
    GL::glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);

    GL::glTexImage2D(
        GL_TEXTURE_2D,          // Target
        0,                      // Level, 0 = base, no minimap,
        GL_RGBA,                // Internal format
        pBitmapData->Width(),   // Width
        pBitmapData->Height(),  // Height
        0,                      // Border
        GL_RGBA,                // Format
        GL_UNSIGNED_BYTE,       // Type
        pBitmapData->Data()     // Bitmap raw data
    );
}

void Texture2d::BindToShaderProgramCore(IShaderProgram* pShaderProgram)
{
    mTexAttribLoc = pShaderProgram->GetShaderParameterIndex("mainTexture");
}

void Texture2d::ActivateCore(void) const
{
    GL::glActiveTexture(GL_TEXTURE0);
    GL::glBindTexture(GL_TEXTURE_2D, mTextureId);
    GL::glUniform1i(mTexAttribLoc, 0); // Bound to GL_TEXTURE0.
}
